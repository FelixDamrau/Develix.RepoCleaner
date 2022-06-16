using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Model;
using Develix.RepoCleaner.Store;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Develix.RepoCleaner.Utils;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner;

public class App
{
    private const string CredentialName = "Develix:RepoCleanerAzureDevopsToken";

    private readonly IStore store;
    private readonly IDispatcher dispatcher;
    private readonly IState<RepositoryInfoState> repositoryInfoState;
    private readonly IState<ConsoleSettingsState> consoleSettingsState;

    public App(
        IStore store,
        IDispatcher dispatcher,
        IState<RepositoryInfoState> repositoryInfoState,
        IState<ConsoleSettingsState> consoleSettingsState)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        this.repositoryInfoState = repositoryInfoState ?? throw new ArgumentNullException(nameof(repositoryInfoState));
        this.consoleSettingsState = consoleSettingsState ?? throw new ArgumentNullException(nameof(consoleSettingsState));
    }

    public async Task Run(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        try
        {
            if (consoleArguments.Config)
            {
                Config();
                return;
            }

            await InitConsole(consoleArguments, appSettings);

            var renderer = new ConsoleRenderer(repositoryInfoState, consoleSettingsState);
            renderer.Show();

            if (consoleSettingsState.Value.ShowDeletePrompt)
            {
                var branchesToDelete = GetBranchesToDelete();
                Delete(branchesToDelete);
            }

        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    private void Config()
    {
        var token = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]azure devops token[/]")
            .PromptStyle("red")
            .Secret());
        AnsiConsole.Status().Start("Storing credentials",
            async (ctx) =>
            {
                var action = new ConfigureCredentialsAction(CredentialName, token);
                dispatcher.Dispatch(action);
                await AsyncHelper.WaitUntilAsync(() => !consoleSettingsState.Value.Configuring, 100, 2000, default);
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

    private async Task InitConsole(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        await AnsiConsole.Progress()
            .Columns(GetProgressColumns())
            .AutoClear(true)
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Initializing");
                await InitProgress(task);
                LoadDataProgress(consoleArguments, appSettings, task);
                await AuthenticateProgress(task);
                await InitRepositoryProgress(consoleArguments, task);

                task.Description = "Finished";
            });
    }

    private async Task InitProgress(ProgressTask task)
    {
        await store.InitializeAsync();
        task.Increment(5);
    }

    private void LoadDataProgress(ConsoleArguments consoleArguments, AppSettings appSettings, ProgressTask task)
    {
        task.Description = "Loading Data";
        dispatcher.Dispatch(new SetConsoleSettingsAction(consoleArguments, appSettings));
        task.Increment(2);
    }

    private async Task AuthenticateProgress(ProgressTask task)
    {
        task.Description = "Authenticating";
        dispatcher.Dispatch(new LoginServicesAction(CredentialName));
        task.Increment(10);
        await AsyncHelper.WaitUntilAsync(() => repositoryInfoState.Value.WorkItemServiceConnected, 100, 30000, default);
    }

    private async Task InitRepositoryProgress(ConsoleArguments consoleArguments, ProgressTask task)
    {
        task.Description = "Init repository";
        dispatcher.Dispatch(new InitRepositoryAction(consoleArguments.Branches));
        task.Increment(3);
        var waitRepositoryLoaded = AsyncHelper.WaitUntilAsync(() => repositoryInfoState.Value.RepositoryLoaded, 100, 30000, default)
            .ContinueWith(t => task.Increment(50));
        var waitWorkItemsLoaded = AsyncHelper.WaitUntilAsync(() => repositoryInfoState.Value.WorkItemsLoaded, 100, 30000, default)
            .ContinueWith(t => task.Increment(50));
        await Task.WhenAll(waitRepositoryLoaded, waitWorkItemsLoaded);
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
}
