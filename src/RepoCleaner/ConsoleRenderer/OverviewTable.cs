using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
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
        var tableRows = GetTableRows(teamProjects.Count);
        var table = CreateTable(tableRows.FirstOrDefault());
        foreach (var row in tableRows.Select(tr => tr.GetRowData()))
            table.AddRow(row);

        var panel = new Panel(table)
            .Header($"Branches ({string.Join(", ", teamProjects)})")
            .Border(BoxBorder.Rounded);
        return panel;

    }

    private List<OverviewTableRowBase> GetTableRows(int numberOfTeamProject)
    {
        return repositoryInfoState.Repository
            .Branches
            .Select(b => GetTableRow(b, numberOfTeamProject))
            .ToList();

        OverviewTableRowBase GetTableRow(Branch branch, int numberOfTeamProjects)
        {
            return numberOfTeamProjects switch
            {
                1 => new OverviewTableRow(branch, GetRelatedWorkItem(branch)),
                > 1 => new OverviewTableRowWithProject(branch, GetRelatedWorkItem(branch)),
                _ => throw new InvalidOperationException($"Well, this is really unexpected!"),
            };
        }
        WorkItem? GetRelatedWorkItem(Branch b) => repositoryInfoState.WorkItems.FirstOrDefault(wi => wi.Id == b.RelatedWorkItemId);
    }

    private static Table CreateTable(OverviewTableRowBase? rowTemplate)
    {
        var table = new Table();
        table.Border(TableBorder.None);
        foreach (var columnTitle in rowTemplate?.GetColumns() ?? Array.Empty<string>())
            table.AddColumn($"[bold]{columnTitle}[/]");

        return table;
    }
}
