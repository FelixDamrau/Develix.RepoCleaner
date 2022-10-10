﻿using Develix.AzureDevOps.Connector.Model;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTablePullRequest : OverviewTableRowCustomBase
{
    private readonly PullRequest pullRequest;

    public OverviewTablePullRequest(OverviewTableRowBase parentRow, PullRequest pullRequest)
        : base(parentRow, ":right_arrow_curving_down:", GetPullRequestTitle(pullRequest))
    {
        this.pullRequest = pullRequest;
    }

    public override IEnumerable<Markup> GetRowData()
    {
        var rowData = base.GetRowData().ToList();
        var wiStatusColumIndex = GetColumIndex(nameof(OverviewTableRow.WorkItemStatusString));
        rowData[wiStatusColumIndex] = new Markup(GetStatus(pullRequest.Status));
        return rowData;
    }

    private static string GetPullRequestTitle(PullRequest pullRequest)
    {
        return $"{pullRequest.Id}: {pullRequest.Title.EscapeMarkup()}";
    }

    private static string GetStatus(PullRequestStatus status)
    {
        var color = status switch
        {
            PullRequestStatus.Invalid => "red",
            PullRequestStatus.Active => "deepskyblue3_1",
            PullRequestStatus.Abandoned => "orange3",
            PullRequestStatus.Completed => "green",
            _ => throw new NotImplementedException($"The status '{status}' is not supported!"),
        };
        return $"[{color}]⬤[/]";
    }
}
