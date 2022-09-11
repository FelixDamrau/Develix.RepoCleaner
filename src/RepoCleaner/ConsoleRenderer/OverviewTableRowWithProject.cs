using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleRenderer;
internal class OverviewTableRowWithProject : OverviewTableRow
{
    [OverviewTableColumn("Proj", 25)]
    public string TeamProject { get; }

    public OverviewTableRowWithProject(Branch branch, WorkItem? relatedWorkItem)
        : base(branch, relatedWorkItem)
    {
        TeamProject = GetTeamProject(relatedWorkItem);
    }

    private static string GetTeamProject(WorkItem? relatedWorkItem)
    {
        return relatedWorkItem is not null
            ? relatedWorkItem.TeamProject.EscapeMarkup()
            : ":minus:";
    }
}
