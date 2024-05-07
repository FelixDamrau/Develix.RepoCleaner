using System.Diagnostics;
using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace Develix.RepoCleaner.Git.External;

internal class GitHandler : IGitHandler
{
    public IReadOnlyList<Result> DeleteBranches(string repositoryPath, IEnumerable<Branch> branches)
    {
        throw new NotImplementedException();
    }

    public Result<Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches)
    {
        var branches = GetBranches(path);
        var repository = new Repository(path);
        foreach (var branch in branches.Where(b => !b.IsRemote))
            repository.AddBranch(branch);

        return Result.Ok(repository);
    }

    public Result<Repository> GetRemoteRepository(string path, IEnumerable<string> excludedBranches)
    {
        var branches = GetBranches(path);
        var repository = new Repository(path);
        foreach (var branch in branches.Where(b => b.IsRemote))
            repository.AddBranch(branch);

        return Result.Ok(repository);
    }

    public Result<Repository> GetRepository(string path, IEnumerable<string> excludedBranches)
    {
        var branches = GetBranches(path);
        var repository = new Repository(path);
        foreach (var branch in branches.Where(b => !b.IsRemote))
            repository.AddBranch(branch);

        return Result.Ok(repository);
    }

    private IEnumerable<Branch> GetBranches(string path)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git.exe",
            Arguments = "branch --format \"%(HEAD)||%(refname)||%(refname:short)||%(upstream:track)||%(contents:subject)||%(authorname)\" --all",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            WorkingDirectory = path
        };

        var p = Process.Start(psi) ?? throw new UnreachableException("The git process could not start");
        p.WaitForExit();

        return ParseBranchOutput(p.StandardOutput);
    }

    private IEnumerable<Branch> ParseBranchOutput(StreamReader standardOutput)
    {
        while (standardOutput.ReadLine() is { } line)
        {
            var split = line.Split("||");
            var head = split[0].Trim();
            var refName = split[1].Trim();
            var friendlyName = split[2].Trim();
            var upstreamTrack = split[3].Trim();
            var subject = split[4].Trim();
            var author = split[5].Trim();
            
            yield return new Branch()
            {
                FriendlyName = friendlyName,
                HeadCommitAuthor = author,
                HeadCommitDate = DateTimeOffset.MinValue,
                IsCurrent = IsCurrent(head),
                IsRemote = IsRemote(refName),
                Name = refName,
                RelatedWorkItemId = WorkItemIdParser.Parse(friendlyName),
                Status = GetTrackingBranchStatus(upstreamTrack),
            };
        }
    }

    private bool IsCurrent(string head) => head == "*";

    private TrackingBranchStatus GetTrackingBranchStatus(string upstreamTrackValue)
    {
        return upstreamTrackValue switch
        {
            "" or null => TrackingBranchStatus.None,
            "[gone]" => TrackingBranchStatus.Deleted,
            _ => TrackingBranchStatus.Active,
        };
    }

    private bool IsRemote(string refName) => refName.StartsWith("refs/remotes");
}
