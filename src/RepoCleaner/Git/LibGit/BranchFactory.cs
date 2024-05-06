using LibGit2Sharp;

namespace Develix.RepoCleaner.Git.LibGit;

internal static class BranchFactory
{
    public static Model.Branch Create(Branch branch)
    {
        return new Model.Branch
        {
            Name = branch.CanonicalName,
            FriendlyName = branch.FriendlyName,
            HeadCommitAuthor = branch.Tip.Author.Name,
            HeadCommitDate = branch.Tip.Author.When,
            Status = GetTrackingBranchStatus(branch),
            RelatedWorkItemId = WorkItemIdParser.Parse(branch.FriendlyName),
            IsRemote = branch.IsRemote,
            IsCurrent = branch.IsCurrentRepositoryHead,
        };
    }

    private static Model.TrackingBranchStatus GetTrackingBranchStatus(Branch branch)
    {
        if (branch.TrackedBranch is null)
            return Model.TrackingBranchStatus.None;
        if (branch.TrackedBranch.Reference?.TargetIdentifier is null)
            return Model.TrackingBranchStatus.Deleted;
        return Model.TrackingBranchStatus.Active;
    }
}
