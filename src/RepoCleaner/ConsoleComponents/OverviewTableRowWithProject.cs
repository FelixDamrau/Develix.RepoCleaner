using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTableRowWithProject(
    Branch branch,
    WorkItem? relatedWorkItem,
    IReadOnlyDictionary<string, string> workItemTypeIcons,
    IReadOnlyDictionary<string, string> shortProjectNames)
    : OverviewTableRow(branch, relatedWorkItem, workItemTypeIcons)
{
    [OverviewTableColumn("Project", 25)]
    public string TeamProject { get; } = GetTeamProject(relatedWorkItem, shortProjectNames);

    private static string GetTeamProject(WorkItem? relatedWorkItem, IReadOnlyDictionary<string, string> shortProjectNames)
    {
        if (relatedWorkItem is null)
            return ":minus:";
        if (shortProjectNames.TryGetValue(relatedWorkItem.TeamProject, out var shortProjectName))
            return shortProjectName;
        return relatedWorkItem.TeamProject.EscapeMarkup();
    }
}
