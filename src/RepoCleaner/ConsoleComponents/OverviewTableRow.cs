using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTableRow : OverviewTableRowBase
{
    [OverviewTableColumn("Name", 10)]
    public string BranchName { get; }

    [OverviewTableColumn("ID", 20)]
    public string WorkItemId { get; }

    [OverviewTableColumn(":white_question_mark:", 30)]
    public string WorkItemTypeString { get; }

    [OverviewTableColumn("WI Title", 40)]
    public string Title { get; }

    [OverviewTableColumn("WI", 50)]
    public string WorkItemStatusString { get; }

    [OverviewTableColumn(":up_arrow:", 60)]
    public string TrackingBranchStatusString { get; }

    public OverviewTableRow(Branch branch, WorkItem? relatedWorkItem, IReadOnlyDictionary<string, string> workItemTypeIcons)
    {
        BranchName = GetBranchName(branch);
        WorkItemId = GetWorkItemId(relatedWorkItem);
        WorkItemTypeString = GetWorkItemType(relatedWorkItem, workItemTypeIcons);
        Title = GetColoredTitle(relatedWorkItem);
        WorkItemStatusString = GetWorkItemStatus(relatedWorkItem);
        TrackingBranchStatusString = GetTrackingBranchStatus(branch);
    }

    private static string GetBranchName(Branch branch)
    {
        var friendlyName = branch switch
        {
            { IsCurrent: true, FriendlyName.Length: > 20 } => ":house: " + branch.FriendlyName[0..19] + "…",
            { IsCurrent: true } => ":house: " + branch.FriendlyName,
            { FriendlyName.Length: > 22 } => branch.FriendlyName[0..21] + "…",
            _ => branch.FriendlyName,
        };
        return friendlyName.EscapeMarkup();
    }

    private static string GetWorkItemId(WorkItem? relatedWorkItem)
    {
        return relatedWorkItem?.AzureDevopsLink is not null
            ? $"[link={relatedWorkItem.AzureDevopsLink.Replace(" ", "%20")}]{relatedWorkItem.Id}[/]"
            : ":minus:";
    }

    private static string GetWorkItemType(WorkItem? relatedWorkItem, IReadOnlyDictionary<string, string> workItemTypeIcons)
    {
        if (relatedWorkItem?.WorkItemType.Name is not { } workItemType)
            return ":minus:";
        if (workItemTypeIcons.TryGetValue(workItemType, out var icon))
            return icon;
        return ":red_question_mark:";
    }

    private static string GetColoredTitle(WorkItem? workItem)
    {
        if (workItem is null)
            return ":minus:";
        var escapedTitle = workItem.Title.EscapeMarkup();
        return $"[#{workItem.WorkItemType.Color}]{escapedTitle}[/]";
    }

    private static string GetWorkItemStatus(WorkItem? relatedWorkItem)
    {
        var color = relatedWorkItem?.Status.Color ?? "808080";
        return $"[#{color}]⬤[/]";
    }

    private static string GetTrackingBranchStatus(Branch branch)
    {
        return branch.Status switch
        {
            TrackingBranchStatus.Active => "[green]⬤[/]",
            TrackingBranchStatus.Invalid => ":cross_mark:",
            TrackingBranchStatus.None => "[#808080]⬤[/]",
            TrackingBranchStatus.Deleted => "[red]⬤[/]",
            _ => throw new NotImplementedException($"The {nameof(TrackingBranchStatus)} '{branch.Status}' is not supported yet!"),
        };
    }
}
