using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner;
public class ConsoleRenderer
{
    private readonly IState<RepositoryInfoState> repositoryInfoState;

    public ConsoleRenderer(IState<RepositoryInfoState> repositoryInfoState)
    {
        this.repositoryInfoState = repositoryInfoState;
    }

    public void Show()
    {
        var table = new Table();
        table
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("WI")
            .AddColumn("Description");

        foreach (var branch in repositoryInfoState.Value.Repository.Branches)
        {
            table
                .AddRow($"{branch.FriendlyName}", ":lady_beetle:", "[blue]Oh boy[/]!");

        }


        AnsiConsole.Write(table);
    }
}
