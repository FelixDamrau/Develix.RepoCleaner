using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleRenderer;

internal class OverviewTableRowWithProject : OverviewTableRow
{
    [OverviewTableColumn("Project", 25)]
    public string TeamProject { get; }

    public OverviewTableRowWithProject(Branch branch, WorkItem? relatedWorkItem, IReadOnlyDictionary<string, string> shortProjectNames)
        : base(branch, relatedWorkItem)
    {
        TeamProject = GetTeamProject(relatedWorkItem, shortProjectNames);
    }

    private static string GetTeamProject(WorkItem? relatedWorkItem, IReadOnlyDictionary<string, string> shortProjectNames)
    {
        if (relatedWorkItem is null)
            return ":minus:";
        if (shortProjectNames.TryGetValue(relatedWorkItem.TeamProject, out var shortProjectName))
            return shortProjectName;
        return relatedWorkItem.TeamProject.EscapeMarkup();
    }
}
