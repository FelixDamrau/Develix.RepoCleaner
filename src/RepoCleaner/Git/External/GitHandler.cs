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
            Arguments = "branch --format \"%(HEAD)\t%(refname)\t%(refname:short)\t%(upstream:track)\t%(authordate:unix)\t%(authorname)\" --all",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            WorkingDirectory = path
        };

        var p = Process.Start(psi) ?? throw new UnreachableException("The git process could not start");
        p.WaitForExit();

        return ParseBranchOutput(p.StandardOutput);
    }

    private static IEnumerable<Branch> ParseBranchOutput(StreamReader standardOutput)
    {
        while (standardOutput.ReadLine() is { } line)
        {
            var split = line.Split("\t");
            var head = split[0].Trim();
            var refName = split[1].Trim();
            var friendlyName = split[2].Trim();
            var upstreamTrack = split[3].Trim();
            var authorDateString = split[4].Trim();
            var author = split[5].Trim();

            yield return new Branch()
            {
                FriendlyName = friendlyName,
                HeadCommitAuthor = author,
                HeadCommitDate = GetAuthorDate(authorDateString),
                IsCurrent = IsCurrent(head),
                IsRemote = IsRemote(refName),
                Name = refName,
                RelatedWorkItemId = WorkItemIdParser.Parse(friendlyName),
                Status = GetTrackingBranchStatus(upstreamTrack),
            };
        }
    }

    private static DateTimeOffset GetAuthorDate(string unixTimeString)
    {
        return long.TryParse(unixTimeString, out var unixTime)
            ? DateTimeOffset.FromUnixTimeSeconds(unixTime)
            : throw new UnreachableException($"The unix time string '{unixTimeString}' could not be parsed to a valid time!");
    }
    private static bool IsCurrent(string head) => head == "*";

    private static TrackingBranchStatus GetTrackingBranchStatus(string upstreamTrackValue)
    {
        return upstreamTrackValue switch
        {
            "" or null => TrackingBranchStatus.None,
            "[gone]" => TrackingBranchStatus.Deleted,
            _ => TrackingBranchStatus.Active,
        };
    }

    private static bool IsRemote(string refName) => refName.StartsWith("refs/remotes");
}
