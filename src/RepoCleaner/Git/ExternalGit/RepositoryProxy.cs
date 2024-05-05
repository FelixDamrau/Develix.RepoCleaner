using Develix.RepoCleaner.Git.Model;
using Spectre.Console;
using BranchNames = (string Name, string FriendlyName);

namespace Develix.RepoCleaner.Git.ExternalGit;

internal class RepositoryProxy
{
    public IEnumerable<string> LocalBranchNames { get; init; } = [];
    public IEnumerable<string> RemoteBranchNames { get; set; } = [];
    public string? CurrentBranchName { set; get; }

    public Repository ToRepository()
    {
        var localBranchProxies = LocalBranchNames.Select(GetLocalFriendlyName).ToList();
        var remoteBranchProxies = RemoteBranchNames.Select(GetRemoteFriendlyName).ToList();
        var localBranches = GetLocalBranches(localBranchProxies, remoteBranchProxies).ToList();
        var remoteBranches = GetRemoteBranches(remoteBranchProxies);
        var currentBranch = localBranches.FirstOrDefault(b => b.Name == CurrentBranchName);

        var repository = new Repository("a",  currentBranch);
        foreach(var branch in localBranches)
            repository.AddBranch(branch);
        foreach(var branch in remoteBranches)
            repository.AddBranch(branch);
        return repository;
    }

    private static IEnumerable<Branch> GetLocalBranches(List<BranchNames> localBranchProxies, List<BranchNames> remoteBranchProxies)
    {
        return localBranchProxies
            .Select(p => new Branch()
            {
                Name = p.Name,
                FriendlyName = p.FriendlyName,
                IsRemote = false,
                RelatedWorkItemId = WorkItemIdParser.TryParse(p.FriendlyName, out var id) ? id : null,
                Status = GetStatus(p, remoteBranchProxies),
            });
    }

    private static IEnumerable<Branch> GetRemoteBranches(List<(string Name, string FriendlyName)> remoteBranchProxies)
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

    private static TrackingBranchStatus GetStatus((string Name, string FriendlyName) p, List<(string Name, string FriendlyName)> remoteBranchProxies)
    {
        return remoteBranchProxies.Select(q => q.FriendlyName).Contains(p.FriendlyName)
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
}
