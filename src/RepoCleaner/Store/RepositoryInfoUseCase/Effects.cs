using Develix.AzureDevOps.Connector.Service;
using Develix.CredentialStore.Win32;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;
public class Effects
{
    private readonly IState<ConsoleSettingsState> consoleSettingsState;
    private readonly IWorkItemService workItemService;
    private readonly IReposService reposService;

    public Effects(IState<ConsoleSettingsState> consoleSettingsState, IWorkItemService workItemService, IReposService reposService)
    {
        this.consoleSettingsState = consoleSettingsState;
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
    public async Task HandleLoginPullRequestServiceAction(LoginWorkItemServiceAction action, IDispatcher dispatcher)
    {
        var credential = CredentialManager.Get(action.CredentialName);
        var result = await workItemService.Initialize(consoleSettingsState.Value.AzureDevOpsUri, credential.Value.Password!);
        dispatcher.Dispatch(new LoginWorkItemServiceResultAction(result));
    }

    [EffectMethod]
    public async Task HandleLoginRepoServiceAction(LoginReposServiceAction action, IDispatcher dispatcher)
    {
        var credential = CredentialManager.Get(action.CredentialName);
        var result = await reposService.Initialize(consoleSettingsState.Value.AzureDevOpsUri, credential.Value.Password!);
        dispatcher.Dispatch(new LoginReposServiceResultAction(result));
    }

    [EffectMethod(typeof(InitRepositoryAction))]
    public Task HandleInitRepositoryAction(IDispatcher dispatcher)
    {
        var path = consoleSettingsState.Value.Path ?? Directory.GetCurrentDirectory();
        var repository = Git.Reader.GetLocalRepo(path);
        var setRepositoryAction = new SetRepositoryAction(repository);
        dispatcher.Dispatch(setRepositoryAction);
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleSetRepositoryAction(SetRepositoryAction action, IDispatcher dispatcher)
    {
        var ids = action.Repository.Branches.Select(b => b.RelatedWorkItemId).OfType<int>();
        var workItemsResult = await workItemService.GetWorkItems(ids, consoleSettingsState.Value.Pr);

        if (workItemsResult.Valid)
        {
            var setWorkItemsAction = new SetWorkItemsAction(workItemsResult.Value.ToList());
            dispatcher.Dispatch(setWorkItemsAction);
        }
    }
}
