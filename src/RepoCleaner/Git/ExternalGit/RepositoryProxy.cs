using System.Text.RegularExpressions;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;
using Spectre.Console;
using BranchNames = (string Name, string FriendlyName);

namespace Develix.RepoCleaner.Git.ExternalGit;

internal class RepositoryProxy
{
    public IEnumerable<string> LocalBranchNames { get; init; } = [];
    public IEnumerable<string> RemoteBranchNames { get; init; } = [];
    public string? CurrentBranchName { get; init; }

    public Repository ToRepository(BranchSourceKind branchSourceKind, IEnumerable<string> excludedBranches)
    {
        var localBranchProxies = LocalBranchNames.Select(GetLocalFriendlyName).ToList();
        var remoteBranchProxies = RemoteBranchNames.Select(GetRemoteFriendlyName).ToList();
        var localBranches = GetLocalBranches(localBranchProxies, remoteBranchProxies, CurrentBranchName).ToList();
        var remoteBranches = GetRemoteBranches(remoteBranchProxies);

        var repository = new Repository("You only need a name to delete stuff...");
        var excludeRegex = GetExcludedBranchesRegex(excludedBranches);
        if (branchSourceKind.HasFlag(BranchSourceKind.Local))
        {
            foreach (var branch in localBranches.Where(b => !IsExcluded(b.FriendlyName, excludeRegex)))
                repository.AddBranch(branch);
        }
        if (branchSourceKind.HasFlag(BranchSourceKind.Remote))
        {
            foreach (var branch in remoteBranches.Where(b => !IsExcluded(b.FriendlyName, excludeRegex)))
                repository.AddBranch(branch);
        }
        return repository;
    }

    private static IEnumerable<Branch> GetLocalBranches(
        List<BranchNames> localBranchProxies,
        List<BranchNames> remoteBranchProxies,
        string? currentBranchName)
    {
        return localBranchProxies
            .Select(p => new Branch()
            {
                Name = p.Name,
                FriendlyName = p.FriendlyName,
                IsCurrent = p.Name == currentBranchName,
                IsRemote = false,
                RelatedWorkItemId = WorkItemIdParser.TryParse(p.FriendlyName, out var id) ? id : null,
                Status = GetStatus(p, remoteBranchProxies),
            });
    }

    private static IEnumerable<Branch> GetRemoteBranches(
        List<BranchNames> remoteBranchProxies)
    {
        return remoteBranchProxies
            .Select(p => new Branch()
            {
                Name = p.Name,
                FriendlyName = p.FriendlyName,
                IsRemote = false,
                Status = TrackingBranchStatus.None,
            });
    }

    private static TrackingBranchStatus GetStatus(BranchNames branchNames, List<BranchNames> remoteBranchProxies)
    {
        return remoteBranchProxies.Select(q => q.FriendlyName).Contains(branchNames.FriendlyName)
            ? TrackingBranchStatus.Active
            : TrackingBranchStatus.None;
    }

    private static BranchNames GetLocalFriendlyName(string filePath) => GetFriendlyName(filePath, RepositoryFactory.LocalBranchesPath);

    private static BranchNames GetRemoteFriendlyName(string filePath) => GetFriendlyName(filePath, RepositoryFactory.RemoteBranchesPath);

    private static BranchNames GetFriendlyName(string filePath, string identifier)
    {
        var index = filePath.IndexOf(identifier);
        return index != -1
            ? (filePath[index..], filePath[(index + identifier.Length)..])
            : (filePath, filePath);
    }

    private static Regex GetExcludedBranchesRegex(IEnumerable<string> excludedBranches) => new($"(?:{string.Join('|', excludedBranches)})");

    private static bool IsExcluded(string branchName, Regex excludedBranchesRegex) => excludedBranchesRegex.IsMatch(branchName);
}
