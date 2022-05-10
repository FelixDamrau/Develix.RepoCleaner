using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner;
public class ConsoleRenderer
{
    private readonly IState<RepositoryInfoState> repositoryInfoState;
    private readonly IState<ConsoleSettingsState> consoleSettingsState;

    public ConsoleRenderer(IState<RepositoryInfoState> repositoryInfoState, IState<ConsoleSettingsState> consoleSettingsState)
    {
        this.repositoryInfoState = repositoryInfoState ?? throw new ArgumentNullException(nameof(repositoryInfoState));
        this.consoleSettingsState = consoleSettingsState ?? throw new ArgumentNullException(nameof(consoleSettingsState));
    }

    public void Show()
    {
        var table = CreateTable();

        foreach (var branch in repositoryInfoState.Value.Repository.Branches)
        {
            var relatedWorkItem = repositoryInfoState.Value.WorkItems.FirstOrDefault(wi => wi.Id == branch.RelatedWorkItemId);
            table.AddRow(GetRowData(branch, relatedWorkItem).ToArray());

            if (consoleSettingsState.Value.Pr && relatedWorkItem is not null)
            {
                AddPullRequests(table, relatedWorkItem.PullRequests);
            }
        }

        var panel = new Panel(table).Header("Branches").Border(BoxBorder.Rounded);
        AnsiConsole.Write(panel);
    }

    private static Table CreateTable()
    {
        var table = new Table();
        table
            .Border(TableBorder.None)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]ID[/]")
            .AddColumn("[bold]:white_question_mark:[/]")
            .AddColumn("[bold]WI Title[/]")
            .AddColumn("[bold]WI[/]")
            .AddColumn("[bold]:up_arrow:[/]");

        return table;
    }

    private IEnumerable<string> GetRowData(Branch branch, WorkItem? relatedWorkItem)
    {
        yield return GetBranchName(branch.FriendlyName);
        yield return GetWorkItemId(branch.RelatedWorkItemId);
        yield return GetWorkItemType(relatedWorkItem);
        yield return GetColoredTitle(relatedWorkItem);
        yield return GetWorkItemStatus(relatedWorkItem);
        yield return GetTrackingBranchStatus(branch);
    }

    private void AddPullRequests(Table table, IReadOnlyList<PullRequest> pullRequests)
    {
        foreach (var pullRequest in pullRequests)
        {
            var pullRequestLabel = $"[grey]{pullRequest.Title.EscapeMarkup()} | Status:[/] {GetPullRequestStatus(pullRequest)}";
            table.AddRow(string.Empty, $"[grey] {pullRequest.Id}[/]", $"[grey]:right_arrow_curving_down:[/]", pullRequestLabel, string.Empty, string.Empty);
        }
    }

    private string GetBranchName(string friendlyName)
    {
        return friendlyName.Length > 20
            ? friendlyName[0..19] + "…"
            : friendlyName;
    }

    private string GetWorkItemId(int? relatedWorkItemId)
    {
        return relatedWorkItemId is { } value ? value.ToString() : ":minus:";
    }

    private string GetWorkItemType(WorkItem? relatedWorkItem)
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

    private string GetColoredTitle(WorkItem? workItem)
    {
        return workItem?.WorkItemType switch
        {
            null => ":minus:",
            WorkItemType.Invalid => workItem.Title,
            WorkItemType.Bug => $"[red3_1]{workItem.Title}[/]",
            WorkItemType.Epic => $"[darkorange]{workItem.Title}[/]",
            WorkItemType.Feature => $"[mediumvioletred]{workItem.Title}[/]",
            WorkItemType.Impediment => $"[darkviolet_1]{workItem.Title}[/]",
            WorkItemType.ProductBacklogItem => $"[deepskyblue2]{workItem.Title}[/]",
            WorkItemType.Task => $"[yellow3_1]{workItem.Title}[/]",
            WorkItemType.Unknown => workItem.Title,
            _ => throw new NotImplementedException($"The {nameof(WorkItemType)} '{workItem.WorkItemType}' is not supported yet!"),
        };
    }

    private string GetWorkItemStatus(WorkItem? relatedWorkItem)
    {
        return relatedWorkItem?.Status switch
        {
            null or
            WorkItemStatus.Invalid => ":black_circle:",
            WorkItemStatus.New or
            WorkItemStatus.Approved or
            WorkItemStatus.Committed or
            WorkItemStatus.InProgress or
            WorkItemStatus.Open or
            WorkItemStatus.ToDo => ":red_circle:",
            WorkItemStatus.Done or
            WorkItemStatus.Removed or
            WorkItemStatus.Closed => ":green_circle:",
            _ => throw new NotImplementedException($"The {nameof(WorkItemStatus)} '{relatedWorkItem.Status}' is not supported yet!"),
        };
    }

    private string GetTrackingBranchStatus(Branch branch)
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

    private string GetPullRequestStatus(PullRequest pullRequest)
    {
        return pullRequest.Status switch
        {
            PullRequestStatus.Invalid => ":cross_mark:",
            PullRequestStatus.Active => ":yellow_circle:",
            PullRequestStatus.Abandoned => ":red_circle:",
            PullRequestStatus.Completed => ":green_circle:",
            _ => throw new NotImplementedException($"The {nameof(PullRequestStatus)} '{pullRequest.Status}' is not supported yet!"),
        };
    }
}
