using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTableRow(Branch branch, WorkItem? relatedWorkItem, IReadOnlyDictionary<string, string> workItemTypeIcons)
    : OverviewTableRowBase
{
    [OverviewTableColumn("Name", 10)]
    public string BranchName { get; } = GetBranchName(branch);

    [OverviewTableColumn("ID", 20)]
    public string WorkItemId { get; } = GetWorkItemId(relatedWorkItem);

    [OverviewTableColumn(":white_question_mark:", 30)]
    public string WorkItemTypeString { get; } = GetWorkItemType(relatedWorkItem, workItemTypeIcons);

    [OverviewTableColumn("WI Title", 40)]
    public string Title { get; } = GetColoredTitle(relatedWorkItem);

    [OverviewTableColumn("WI", 50)]
    public string WorkItemStatusString { get; } = GetWorkItemStatus(relatedWorkItem);

    [OverviewTableColumn(":up_arrow:", 60)]
    public string TrackingBranchStatusString { get; } = GetTrackingBranchStatus(branch);

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
        return $"[#{workItem.WorkItemType.Color.Rgb}]{escapedTitle}[/]";
    }

    private static string GetWorkItemStatus(WorkItem? relatedWorkItem)
    {
        var color = relatedWorkItem?.Status.Color.Rgb is { } rgb ? $"#{rgb}" : "grey30";
        return $"[{color}]⬤[/]";
    }

    private static string GetTrackingBranchStatus(Branch branch)
    {
        return branch.Status switch
        {
            TrackingBranchStatus.Active => "[green]⬤[/]",
            TrackingBranchStatus.Invalid => ":cross_mark:",
            TrackingBranchStatus.None => "[grey30]⬤[/]",
            TrackingBranchStatus.Deleted => "[red]⬤[/]",
            _ => throw new NotImplementedException($"The {nameof(TrackingBranchStatus)} '{branch.Status}' is not supported yet!"),
        };
    }
}
