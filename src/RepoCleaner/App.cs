using Develix.RepoCleaner.ConsoleComponents;
using Develix.RepoCleaner.Model;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
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
            await store.InitializeAsync();

            if (consoleArguments.Config)
            {
                Config.Show(CredentialName, consoleSettingsState, dispatcher);
                return;
            }

            await InitConsole(consoleArguments, appSettings);
            LogErrors(repositoryInfoState);
            ShowOverviewTable();
            if (consoleSettingsState.Value.ShowDeletePrompt)
                ShowDeletePrompt();

        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    private async Task InitConsole(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        var initConsole = new Init(CredentialName, dispatcher, repositoryInfoState, store);
        await initConsole.Execute(consoleArguments, appSettings);
    }

    private static void LogErrors(IState<RepositoryInfoState> repositoryInfoState)
    {
        foreach (var error in repositoryInfoState.Value.ErrorMessages)
        {
            AnsiConsole.MarkupLine(error);
        }
    }

    private void ShowOverviewTable()
    {
        var overviewTable = new OverviewTable(repositoryInfoState.Value, consoleSettingsState.Value);
        AnsiConsole.Write(overviewTable.GetOverviewTable());
    }

    private void ShowDeletePrompt()
    {
        var branchesToDelete = Delete.Prompt(repositoryInfoState);
        Delete.Execute(branchesToDelete, repositoryInfoState);
    }
}
