using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTable
{
    private readonly RepositoryInfoState repositoryInfoState;
    private readonly ConsoleSettingsState consoleSettingsState;

    public OverviewTable(RepositoryInfoState repositoryInfoState, ConsoleSettingsState consoleSettingsState)
    {
        this.repositoryInfoState = repositoryInfoState ?? throw new ArgumentNullException(nameof(repositoryInfoState));
        this.consoleSettingsState = consoleSettingsState ?? throw new ArgumentNullException(nameof(consoleSettingsState));
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

        return GetDisplay(teamProjects, table);
    }

    private IReadOnlyList<OverviewTableRowBase> GetTableRows(int numberOfTeamProject)
    {
        return repositoryInfoState.Repository
            .Branches
            .Select(b => GetTableRow(b, numberOfTeamProject))
            .SelectMany(x => x)
            .ToList();
    }

    private IEnumerable<OverviewTableRowBase> GetTableRow(Branch branch, int numberOfTeamProjects)
    {
        var workItem = repositoryInfoState.WorkItems.FirstOrDefault(wi => wi.Id == branch.RelatedWorkItemId);
        var dataRow = GetDataRow(branch, numberOfTeamProjects, workItem);
        yield return dataRow;
        foreach (var pullRequest in workItem?.PullRequests.OrderBy(pr => pr.Id).AsEnumerable() ?? Array.Empty<PullRequest>())
        {
            yield return new OverviewTablePullRequest(dataRow, pullRequest);
        }

        if (consoleSettingsState.ShowLastCommitAuthor)
            yield return new OverviewTableRowAuthor(dataRow, branch.HeadCommitAuthor);
    }

    private OverviewTableRowBase GetDataRow(Branch branch, int numberOfTeamProjects, WorkItem? workItem)
    {
        return numberOfTeamProjects switch
        {
            <= 1 => new OverviewTableRow(branch, workItem, consoleSettingsState.WorkItemTypeIcons),
            >= 2 => new OverviewTableRowWithProject(
                branch,
                workItem,
                consoleSettingsState.WorkItemTypeIcons,
                consoleSettingsState.ShortProjectNames),
        };
    }

    private static Table CreateTable(OverviewTableRowBase? rowTemplate)
    {
        var table = new Table();
        table.Border(TableBorder.None);
        foreach (var columnTitle in rowTemplate?.GetColumns() ?? Array.Empty<string>())
            table.AddColumn($"[bold]{columnTitle}[/]");

        return table;
    }

    private static IRenderable GetDisplay(List<string> teamProjects, Table table)
    {
        IRenderable outputDisplay = table.Columns.Count > 0
            ? table
            : new Markup("[bold]No data was found[/]");

        var header = teamProjects.Count > 0
            ? $"[bold]Branches ({string.Join(", ", teamProjects)})[/]"
            : "[bold]Branches[/]";

        var panel = new Panel(outputDisplay)
            .Header(header)
            .Border(BoxBorder.Rounded);

        return panel;
    }
}
