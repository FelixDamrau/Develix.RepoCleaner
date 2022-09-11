using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Develix.RepoCleaner.ConsoleRenderer;
internal class OverviewTable
{
    private readonly RepositoryInfoState repositoryInfoState;

    public OverviewTable(RepositoryInfoState repositoryInfoState)
    {
        this.repositoryInfoState = repositoryInfoState;
    }

    public IRenderable GetOverviewTable()
    {
        var teamProjects = repositoryInfoState.WorkItems
            .Select(wi => wi.TeamProject)
            .Where(tp => !string.IsNullOrWhiteSpace(tp))
            .Distinct()
            .ToList();
        var table = CreateTable(teamProjects);
        var tableRows = repositoryInfoState.Repository
            .Branches
            .Select(b => new OverviewTableRow(b, GetRelatedWorkItem(b)))
            .ToList();
        var tableRowStrings = teamProjects.Count > 1
            ? tableRows.Select(tr => tr.Get())
            : tableRows.Select(tr => tr.GetWithoutProjectColumn());

        foreach (var row in tableRowStrings)
            table.AddRow(row);

        var panel = new Panel(table).Header($"Branches ({string.Join(", ", teamProjects)})");
        return panel;

        WorkItem? GetRelatedWorkItem(Git.Model.Branch b) => repositoryInfoState.WorkItems.FirstOrDefault(wi => wi.Id == b.RelatedWorkItemId);
    }

    private static Table CreateTable(IReadOnlyCollection<string> teamProjects)
    {
        var table = new Table();
        table
            .Border(TableBorder.None)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]ID[/]")
            .AddColumn("[bold]:white_question_mark:[/]");
        if (teamProjects.Count > 1)
            table.AddColumn("[bold]Project[/]");

        table
            .AddColumn("[bold]WI Title[/]")
            .AddColumn("[bold]WI[/]")
            .AddColumn("[bold]:up_arrow:[/]");

        return table;
    }
}
