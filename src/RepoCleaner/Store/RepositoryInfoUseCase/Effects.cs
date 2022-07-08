using Develix.AzureDevOps.Connector.Model;
using Develix.AzureDevOps.Connector.Service;
using Develix.CredentialStore.Win32;
using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Model;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;
public class Effects
{
    private readonly IState<ConsoleSettingsState> consoleSettingsState;
    private readonly IState<RepositoryInfoState> repositoryInfoState;
    private readonly IWorkItemService workItemService;
    private readonly IReposService reposService;

    public Effects(
        IState<ConsoleSettingsState> consoleSettingsState,
        IState<RepositoryInfoState> repositoryInfoState,
        IWorkItemService workItemService,
        IReposService reposService)
    {
        this.consoleSettingsState = consoleSettingsState;
        this.repositoryInfoState = repositoryInfoState;
        this.workItemService = workItemService;
        this.reposService = reposService;
    }

    [EffectMethod]
    public Task HandleLoginServicesAction(LoginServicesAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoginWorkItemServiceAction(action.CredentialName));
        if (consoleSettingsState.Value.Pr)
            dispatcher.Dispatch(new LoginReposServiceAction(action.CredentialName));

        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleLoginWorkItemServiceAction(LoginWorkItemServiceAction action, IDispatcher dispatcher)
    {
        var credential = CredentialManager.Get(action.CredentialName);
        var result = await workItemService.Initialize(consoleSettingsState.Value.AzureDevOpsUri, credential.Value.Password!);
        dispatcher.Dispatch(new LoginWorkItemServiceResultAction(result));
    }

    [EffectMethod]
    public Task HandleLoginWorkItemServiceResultAction(LoginWorkItemServiceResultAction action, IDispatcher dispatcher)
    {
        if (!action.LoginResult.Valid)
        {
            var errorMessage = $"[red]Could not login work item service.[/] " +
                $"Uri: [grey]{consoleSettingsState.Value.AzureDevOpsUri}[/] | " +
                $"Error: [grey]{action.LoginResult.Message}[/]";
            AddErrorMessage(errorMessage, dispatcher);
        }

        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleLoginRepoServiceAction(LoginReposServiceAction action, IDispatcher dispatcher)
    {
        var credential = CredentialManager.Get(action.CredentialName);
        var result = await reposService.Initialize(consoleSettingsState.Value.AzureDevOpsUri, credential.Value.Password!);
        dispatcher.Dispatch(new LoginReposServiceResultAction(result));
    }

    [EffectMethod]
    public Task HandleLoginRepoServiceResultAction(LoginReposServiceResultAction action, IDispatcher dispatcher)
    {
        if (!action.LoginResult.Valid)
        {
            var errorMessage = $"[red]Could not login repo service.[/]" +
                $"Uri: [grey]{consoleSettingsState.Value.AzureDevOpsUri}[/] | " +
                $"Error: [grey]{action.LoginResult.Message}[/]";
            AddErrorMessage(errorMessage, dispatcher);
        }

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task HandleInitRepositoryAction(InitRepositoryAction action, IDispatcher dispatcher)
    {
        var path = consoleSettingsState.Value.Path ?? Directory.GetCurrentDirectory();
        var repositoryResult = action.BranchSourceKind switch
        {
            BranchSourceKind.Local => Reader.GetLocalRepo(path, consoleSettingsState.Value.ExcludedBranches),
            BranchSourceKind.Remote => Reader.GetRemoteRepo(path, consoleSettingsState.Value.ExcludedBranches),
            BranchSourceKind.All => Reader.GetRepo(path, consoleSettingsState.Value.ExcludedBranches),
            _ => throw new NotSupportedException($"The {nameof(BranchSourceKind)} '{action.BranchSourceKind}' is not supported!"),
        };
        if (!repositoryResult.Valid)
            AddErrorMessage($"[red]Failed to init repository![/] Error: [grey]{repositoryResult.Message}[/]", dispatcher);

        var setRepositoryAction = new SetRepositoryAction(repositoryResult.Value);
        dispatcher.Dispatch(setRepositoryAction);
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleSetRepositoryAction(SetRepositoryAction action, IDispatcher dispatcher)
    {
        if (repositoryInfoState.Value.WorkItemServiceState != ServiceConnectionState.Connected)
        {
            Dispatch(Array.Empty<WorkItem>());
            return;
        }

        var ids = action.Repository.Branches.Select(b => b.RelatedWorkItemId).OfType<int>();
        var workItemsResult = await workItemService.GetWorkItems(ids, consoleSettingsState.Value.Pr);

        if (workItemsResult.Valid)
            Dispatch(workItemsResult.Value);
        else
            Dispatch(Array.Empty<WorkItem>());

        void Dispatch(IReadOnlyList<WorkItem> workItems)
        {
            var setWorkItemsAction = new SetWorkItemsAction(workItems);
            dispatcher.Dispatch(setWorkItemsAction);
        }
    }

    private void AddErrorMessage(string errorMessage, IDispatcher dispatcher)
    {
        var addErrorAction = new AddErrorAction(errorMessage);
        dispatcher.Dispatch(addErrorAction);
    }
}
