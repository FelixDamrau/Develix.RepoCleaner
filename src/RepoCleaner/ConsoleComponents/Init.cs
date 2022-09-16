using Develix.RepoCleaner.Model;
using Develix.RepoCleaner.Store;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Develix.RepoCleaner.Utils;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

public class Init
{
    private readonly string credentialName;
    private readonly IDispatcher dispatcher;
    private readonly IState<RepositoryInfoState> repositoryInfoState;

    public Init(string credentialName, IDispatcher dispatcher, IState<RepositoryInfoState> repositoryInfoState, IStore store)
    {
        this.dispatcher = dispatcher;
        this.repositoryInfoState = repositoryInfoState;
        this.credentialName = credentialName;
    }

    public async Task Execute(ConsoleArguments consoleArguments, AppSettings appSettings)
    {
        await AnsiConsole.Status()
            .StartAsync("Loading", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                InitConsoleSettings(consoleArguments, appSettings, ctx);
                await LoginServices(ctx);
                await InitRepository(consoleArguments, ctx);
                ctx.Status = "Done";
            });
    }

    private void InitConsoleSettings(ConsoleArguments consoleArguments, AppSettings appSettings, StatusContext statusContext)
    {
        statusContext.Status = "Initializing Console Settings";
        dispatcher.Dispatch(new SetConsoleSettingsAction(consoleArguments, appSettings));
    }

    private async Task LoginServices(StatusContext statusContext)
    {
        statusContext.Status = "Login Services";
        dispatcher.Dispatch(new LoginServicesAction(credentialName));
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
