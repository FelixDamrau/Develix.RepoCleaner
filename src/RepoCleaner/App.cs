using Develix.RepoCleaner.Store;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;

namespace Develix.RepoCleaner;

public class App
{
    private readonly IStore store;
    private readonly IDispatcher dispatcher;
    private readonly IState<RepositoryInfoState> repositoryInfoState;

    public App(IStore store, IDispatcher dispatcher, IState<RepositoryInfoState> counterState)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        this.repositoryInfoState = counterState ?? throw new ArgumentNullException(nameof(counterState));
    }

    public async void Run(Model.ConsoleSettings consoleSettings)
    {
        await store.InitializeAsync();
        var path = consoleSettings.Path ?? Directory.GetCurrentDirectory();
        var repository = Git.Reader.GetLocalRepo(path);
        var action = new SetRepositoryAction(repository);
        dispatcher.Dispatch(action);

        var renderer = new ConsoleRenderer(repositoryInfoState);
        renderer.Show();

        Console.Read();
    }
}
