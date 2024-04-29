using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.ConsoleComponents.Cli;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTable(AppSettings appSettings, RepoCleanerSettings settings)
{
    private readonly AppSettings appSettings = appSettings;
    private readonly RepoCleanerSettings settings = settings;

    public IRenderable GetOverviewTable(IEnumerable<WorkItem> workItems, Repository repository)
    {
        var teamProjects = workItems
            .Select(wi => wi.TeamProject)
            .Where(tp => !string.IsNullOrWhiteSpace(tp))
            .Distinct()
            .ToList();
        var tableRows = GetTableRows(repository, workItems, teamProjects.Count);
        var table = CreateTable(tableRows.FirstOrDefault());
        foreach (var row in tableRows.Select(tr => tr.GetRowData()))
            table.AddRow(row);

        return GetDisplay(teamProjects, table);
    }

    private List<OverviewTableRowBase> GetTableRows(
        Repository repository,
        IEnumerable<WorkItem> workItems,
        int numberOfTeamProject)
    {
        return repository
            .Branches
            .Select(b => GetTableRow(b, workItems, numberOfTeamProject))
            .SelectMany(tr => tr)
            .ToList();
    }

    private IEnumerable<OverviewTableRowBase> GetTableRow(
        Branch branch,
        IEnumerable<WorkItem> workItems,
        int numberOfTeamProjects)
    {
        var workItem = workItems.FirstOrDefault(wi => wi.Id == branch.RelatedWorkItemId);
        var dataRow = GetDataRow(branch, numberOfTeamProjects, workItem);
        yield return dataRow;

        foreach (var pullRequest in workItem?.PullRequests.OrderBy(pr => pr.Id).AsEnumerable() ?? [])
            yield return new OverviewTablePullRequest(dataRow, pullRequest);

        if (settings.IncludeAuthor)
            yield return new OverviewTableRowAuthor(dataRow, branch.HeadCommitAuthor);
    }

    private OverviewTableRow GetDataRow(Branch branch, int numberOfTeamProjects, WorkItem? workItem)
    {
        return numberOfTeamProjects switch
        {
            <= 1 => new OverviewTableRow(branch, workItem, appSettings.WorkItemTypeIcons),
            >= 2 => new OverviewTableRowWithProject(
                branch,
                workItem,
                appSettings.WorkItemTypeIcons,
                appSettings.ShortProjectNames),
        };
    }

    private static Table CreateTable(OverviewTableRowBase? rowTemplate)
    {
        var table = new Table();
        table.Border(TableBorder.None);
        foreach (var columnTitle in rowTemplate?.GetColumns() ?? [])
            table.AddColumn($"[bold]{columnTitle}[/]");

        return table;
    }

    private static Panel GetDisplay(List<string> teamProjects, Table table)
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
