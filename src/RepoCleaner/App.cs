using Develix.AzureDevOps.Connector.Service;
using Develix.CredentialStore.Win32;
using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Model;
using Develix.RepoCleaner.Store;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner;

public class App
{
    private const string credentialName = "Develix:RepoCleanerAzureDevopsToken";

    private readonly IStore store;
    private readonly IDispatcher dispatcher;
    private readonly IState<RepositoryInfoState> repositoryInfoState;
    private readonly IState<ConsoleSettingsState> consoleSettingsState;
    private readonly IWorkItemService workItemService;

    public App(
        IStore store,
        IDispatcher dispatcher,
        IState<RepositoryInfoState> repositoryInfoState,
        IState<ConsoleSettingsState> consoleSettingsState,
        IWorkItemService workItemService)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        this.repositoryInfoState = repositoryInfoState ?? throw new ArgumentNullException(nameof(repositoryInfoState));
        this.consoleSettingsState = consoleSettingsState ?? throw new ArgumentNullException(nameof(consoleSettingsState));
        this.workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    }

    public async Task Run(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        if (consoleArguments.Config)
            Config();
        await InitConsole(consoleArguments, appSettings);

        var renderer = new ConsoleRenderer(repositoryInfoState);
        renderer.Show();

        var branchesToDelete = GetBranchesToDelete();
        Delete(branchesToDelete);
    }

    private static void Config()
    {
        var token = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]azure devops token[/]")
            .PromptStyle("red")
            .Secret());
        AnsiConsole.Status().Start("Storing credentials",
            (ctx) =>
            {
                var credential = new Credential("token", token, credentialName);
                CredentialManager.CreateOrUpdate(credential);
                ctx.Status("Credentials initialized");
            });
    }

    private IReadOnlyList<string> GetBranchesToDelete()
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Branches to delete?")
                .NotRequired()
                .PageSize(6)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle deletion of a branch, " +
                    "[green]<enter>[/] to deleted selected branches.)[/]")
                .AddChoices(repositoryInfoState.Value.Repository.Branches.Select(b => b.FriendlyName)));
    }

    private void Delete(IReadOnlyList<string> branchesToDelete)
    {
        using var handler = new Handler(repositoryInfoState.Value.Repository);
        foreach (var branchName in branchesToDelete)
        {
            var branch = repositoryInfoState.Value.Repository.Branches.First(b => b.FriendlyName == branchName);
            var result = handler.TryDeleteBranch(branch);
            var message = result.Valid
                ? $"[green]Deleted branch {branchName}[/]"
                : $"[red]Failed:[/] {result.Message}";
            AnsiConsole.MarkupLine(message);
        }
    }

    private static ProgressColumn[] GetProgressColumns()
    {
        return new ProgressColumn[]
        {
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new SpinnerColumn(),
        };
    }

    private async Task InitConsole(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        await AnsiConsole.Progress()
            .Columns(GetProgressColumns())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Initializing");
                await store.InitializeAsync();
                task.Increment(5);

                task.Description = "Loading Data";
                dispatcher.Dispatch(new SetConsoleSettingsAction(consoleArguments, appSettings));
                task.Increment(2);

                var credential = CredentialManager.Get(credentialName);
                var initializeResult = await workItemService.Initialize(consoleSettingsState.Value.AzureDevOpsUri, credential.Value.Password!);
                if (!initializeResult.Valid)
                {
                    AnsiConsole.Markup($"[red]Initialization failed! Error message:[/] {initializeResult.Message}");
                    Environment.Exit(-1);
                }
                task.Increment(10);

                task.Description = "Authenticating";
                dispatcher.Dispatch(new InitRepositoryAction());
                task.Increment(3);
                bool incWi = true;
                bool incRe = true;
                while (!ctx.IsFinished)
                {
                    if (repositoryInfoState.Value.WorkItemsLoaded && incWi)
                    {
                        task.Increment(50);
                        incWi = false;
                    }
                    if (repositoryInfoState.Value.RepositoryLoaded && incRe)
                    {
                        task.Increment(30);
                        incRe = false;
                    }
                    await Task.Delay(100);
                }
                task.Description = "Finished";
            });
    }
}
