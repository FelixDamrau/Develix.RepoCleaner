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

    public OverviewTableRow(Branch branch, WorkItem? relatedWorkItem)
    {
        BranchName = GetBranchName(branch);
        WorkItemId = GetWorkItemId(relatedWorkItem);
        WorkItemTypeString = GetWorkItemType(relatedWorkItem);
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
        return relatedWorkItem is not null
            ? $"[link={relatedWorkItem.AzureDevopsLink}]{relatedWorkItem.Id}[/]"
            : ":minus:";
    }

    private static string GetWorkItemType(WorkItem? relatedWorkItem)
    {
        return relatedWorkItem?.WorkItemType switch
        {
            null => ":minus:",
            WorkItemType.Invalid => ":cross_mark:",
            WorkItemType.Bug => ":lady_beetle:",
            WorkItemType.Epic => ":star:",
            WorkItemType.Feature => ":trophy:",
            WorkItemType.Impediment => ":red_triangle_pointed_up:",
            WorkItemType.ProductBacklogItem => ":notebook:",
            WorkItemType.Task => ":spiral_notepad:",
            WorkItemType.Unknown => ":red_question_mark:",
            _ => throw new NotImplementedException($"The {nameof(WorkItemType)} '{relatedWorkItem.WorkItemType}' is not supported yet!"),
        };
    }

    private static string GetColoredTitle(WorkItem? workItem)
    {
        if (workItem is null)
            return ":minus:";
        var escapedTitle = workItem.Title.EscapeMarkup();
        return workItem.WorkItemType switch
        {
            WorkItemType.Invalid => escapedTitle,
            WorkItemType.Bug => $"[red3_1]{escapedTitle}[/]",
            WorkItemType.Epic => $"[darkorange]{escapedTitle}[/]",
            WorkItemType.Feature => $"[mediumvioletred]{escapedTitle}[/]",
            WorkItemType.Impediment => $"[darkviolet_1]{escapedTitle}[/]",
            WorkItemType.ProductBacklogItem => $"[deepskyblue2]{escapedTitle}[/]",
            WorkItemType.Task => $"[yellow3_1]{escapedTitle}[/]",
            WorkItemType.Unknown => escapedTitle,
            _ => throw new NotImplementedException($"The {nameof(WorkItemType)} '{workItem.WorkItemType}' is not supported yet!"),
        };
    }

    private static string GetWorkItemStatus(WorkItem? relatedWorkItem)
    {
        return relatedWorkItem?.Status switch
        {
            null or
            WorkItemStatus.Invalid => ":black_circle:",
            WorkItemStatus.New or
            WorkItemStatus.Approved or
            WorkItemStatus.Open or
            WorkItemStatus.ToDo => ":red_circle:",
            WorkItemStatus.Committed or
            WorkItemStatus.InProgress => ":yellow_circle:",
            WorkItemStatus.Done or
            WorkItemStatus.Removed or
            WorkItemStatus.Closed => ":green_circle:",
            _ => throw new NotImplementedException($"The {nameof(WorkItemStatus)} '{relatedWorkItem.Status}' is not supported yet!"),
        };
    }

    private static string GetTrackingBranchStatus(Branch branch)
    {
        return branch.Status switch
        {
            TrackingBranchStatus.Active => ":green_circle:",
            TrackingBranchStatus.Invalid => ":cross_mark:",
            TrackingBranchStatus.None => ":black_circle:",
            TrackingBranchStatus.Deleted => ":red_circle:",
            _ => throw new NotImplementedException($"The {nameof(TrackingBranchStatus)} '{branch.Status}' is not supported yet!"),
        };
    }
}
