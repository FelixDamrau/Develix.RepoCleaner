using Develix.AzureDevOps.Connector.Service;
using Develix.CredentialStore.Win32;
using Develix.RepoCleaner.Model;
using Develix.RepoCleaner.Store;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner;

public class App
{
    private readonly IStore store;
    private readonly IDispatcher dispatcher;
    private readonly IState<RepositoryInfoState> repositoryInfoState;
    private readonly IState<ConsoleSettingsState> consoleSettingsState;
    private readonly IWorkItemService workItemService;

    public App(IStore store, IDispatcher dispatcher, IState<RepositoryInfoState> repositoryInfoState, IWorkItemService workItemService)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        this.repositoryInfoState = repositoryInfoState ?? throw new ArgumentNullException(nameof(repositoryInfoState));
        this.workItemService = workItemService;
    }

    public async Task Run(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        // apsettings.json
        if (consoleArguments.Config)
            consoleArguments = Config(consoleArguments);
        await InitConsole(consoleArguments, appSettings);

        var renderer = new ConsoleRenderer(repositoryInfoState);
        renderer.Show();

        var branchesToDelete = GetBranchesToDelete(); // TODO

        Console.Read();
    }

    private ConsoleArguments Config(ConsoleArguments consoleArguments)
    {

        return consoleArguments;
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
                dispatcher.Dispatch(new InitRepositoryAction());
                task.Increment(3);

                task.Description = "Authenticating";
                var credential = CredentialManager.Get("Develix:RepoCleanerAzureDevopsToken");
                _ = await workItemService.Initialize(consoleSettingsState.Value.AzureDevOpsUri, credential.Value.Password!);
                task.Increment(10);

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
