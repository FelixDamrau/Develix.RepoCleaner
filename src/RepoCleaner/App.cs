using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Git.Model;
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
            LogErrors(repositoryInfoState);
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

    private IReadOnlyList<Branch> GetBranchesToDelete()
    {
        var deletableBranches = repositoryInfoState.Value.Repository.Branches.Where(b => IsDeletable(b)).ToList();

        if (deletableBranches.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No branches can be deleted.[/]");
            return Array.Empty<Branch>();
        }
        var instructionText =
            "[grey](Press [blue]<space>[/] to toggle deletion of a branch, " +
            "[green]<enter>[/] to delete selected branches.)[/]";
        var nonDeletableCount = repositoryInfoState.Value.Repository.Branches.Count - deletableBranches.Count;
        if (nonDeletableCount > 1) // current branch is never deletable
            instructionText += $"{Environment.NewLine}[grey]Remote braches cannot be deleted and are not shown here[/]";
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<Branch>()
                .UseConverter((b) => GetDisplayText(b))
                .Title("Branches to delete?")
                .NotRequired()
                .PageSize(6)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .InstructionsText(instructionText)
                .AddChoices(deletableBranches));

        bool IsDeletable(Branch b) => !b.IsRemote && !IsCurrentBranch(b);
        bool IsCurrentBranch(Branch branch) => repositoryInfoState.Value.Repository.CurrentBranch.Name == branch.Name;
        string GetDisplayText(Branch branch)
        {
            var workItem = repositoryInfoState.Value.WorkItems.FirstOrDefault(wi => wi.Id == branch.RelatedWorkItemId);
            var displayText = workItem is null
                ? branch.FriendlyName
                : $"{branch.FriendlyName} [{workItem.Title}]";
            return displayText.EscapeMarkup();
        }
    }

    private void Delete(IReadOnlyList<Branch> branchesToDelete)
    {
        using var handler = new Handler(repositoryInfoState.Value.Repository);
        foreach (var branch in branchesToDelete)
        {
            var result = handler.TryDeleteBranch(branch);
            var message = result.Valid
                ? $"[green]Deleted branch {branch.FriendlyName}[/]"
                : $"[red]Failed:[/] {result.Message}";
            AnsiConsole.MarkupLine(message);
        }
    }

    private async Task InitConsole(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        await AnsiConsole.Status()
            .StartAsync("Loading", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                await InitStore(ctx);
                InitConsoleSettings(consoleArguments, appSettings, ctx);
                await LoginServices(ctx);
                await InitRepository(consoleArguments, ctx);
                ctx.Status = "Done";
            });
    }

    private void LogErrors(IState<RepositoryInfoState> repositoryInfoState)
    {
        foreach (var error in repositoryInfoState.Value.ErrorMessages)
        {
            AnsiConsole.MarkupLine(error);
        }
    }

    private async Task InitStore(StatusContext statusContext)
    {
        statusContext.Status = "Initializing";
        await store.InitializeAsync();
    }

    private void InitConsoleSettings(ConsoleArguments consoleArguments, AppSettings appSettings, StatusContext statusContext)
    {
        statusContext.Status = "Initializing Console Settings";
        dispatcher.Dispatch(new SetConsoleSettingsAction(consoleArguments, appSettings));
    }

    private async Task LoginServices(StatusContext statusContext)
    {
        statusContext.Status = "Login Services";
        dispatcher.Dispatch(new LoginServicesAction(CredentialName));
        await AsyncHelper.WaitUntilAsync(
            () => IsFinalState(repositoryInfoState.Value.WorkItemServiceState),
            100,
            30000,
            default);

        static bool IsFinalState(ServiceConnectionState state) => state == ServiceConnectionState.Connected || state == ServiceConnectionState.FailedToConnect;
    }

    private async Task InitRepository(ConsoleArguments consoleArguments, StatusContext statusContext)
    {
        statusContext.Status = "Initializing repository and work items";
        dispatcher.Dispatch(new InitRepositoryAction(consoleArguments.Branches));
        var waitRepositoryLoaded = AsyncHelper.WaitUntilAsync(() => repositoryInfoState.Value.RepositoryLoaded, 100, 30000, default)
            .ContinueWith(t => statusContext.Status = "Initializing work items");
        var waitWorkItemsLoaded = AsyncHelper.WaitUntilAsync(() => repositoryInfoState.Value.WorkItemsLoaded, 100, 30000, default)
            .ContinueWith(t => statusContext.Status = "Initializing repository");
        await Task.WhenAll(waitRepositoryLoaded, waitWorkItemsLoaded);
    }
}
