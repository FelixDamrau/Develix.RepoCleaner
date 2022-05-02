namespace Develix.RepoCleaner.Git.Model;

public class Branch
{
    public string FriendlyName { get; init; } = "Unknown";

    public string Name { get; init; } = "Unknown";

    public string HeadCommitAuthor { get; init; } = "Unknown";

    public TrackingBranchStatus Status { get; init; }

    public DateTimeOffset HeadCommitDate { get; init; }

    public int? RelatedWorkItemId { get; init; }
}
