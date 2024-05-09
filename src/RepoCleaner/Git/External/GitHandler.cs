using System.Diagnostics;
using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;

namespace Develix.RepoCleaner.Git.External;

internal class GitHandler : IGitHandler
{
    public IReadOnlyList<Result> DeleteBranches(string repositoryPath, IEnumerable<Branch> branches)
    {
        var results = new List<Result>();
        foreach (var branch in branches)
        {
            var result = DeleteBranch(repositoryPath, branch);
            results.Add(result);
        }
        return results;
    }

    public Result<Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repository = new Repository(path);
        var branchesResult = GetBranches(path);
        if (!branchesResult.Valid)
            return Result.Fail<Repository>(branchesResult.Message);

        foreach (var branch in branchesResult.Value.Where(b => !b.IsRemote).Filter(excludedBranches))
            repository.AddBranch(branch);

        return Result.Ok(repository);
    }

    public Result<Repository> GetRemoteRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repository = new Repository(path);
        var branchesResult = GetBranches(path);
        if (!branchesResult.Valid)
            return Result.Fail<Repository>(branchesResult.Message);

        foreach (var branch in branchesResult.Value.Where(b => b.IsRemote).Filter(excludedBranches))
            repository.AddBranch(branch);

        return Result.Ok(repository);
    }

    public Result<Repository> GetRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repository = new Repository(path);
        var branchesResult = GetBranches(path);
        if (!branchesResult.Valid)
            return Result.Fail<Repository>(branchesResult.Message);

        foreach (var branch in branchesResult.Value.Filter(excludedBranches))
            repository.AddBranch(branch);

        return Result.Ok(repository);
    }

    private static Result DeleteBranch(string repositoryPath, Branch branch)
    {
        var arguments = $"branch -D {branch.FriendlyName}";
        var processStartInfo = GetGitProcessStartInfo(repositoryPath, arguments);

        var process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("The git process could not start");
        process.WaitForExit();

        return process.ExitCode == 0
            ? Result.Ok()
            : Result.Fail(process.StandardError.ReadToEnd());
    }

    private static Result<IEnumerable<Branch>> GetBranches(string path)
    {
        var processStartInfo = GetGitProcessStartInfo(
            path,
            "branch --format \"%(HEAD)\t%(refname)\t%(refname:short)\t%(upstream:track)\t%(authordate:unix)\t%(authorname)\" --all");

        var process = Process.Start(processStartInfo) ?? throw new UnreachableException("The git process could not start");
        process.WaitForExit();

        return process.ExitCode != 0
            ? Result.Fail<IEnumerable<Branch>>(process.StandardError.ReadToEnd())
            : Result.Ok(ParseBranchOutput(process.StandardOutput));
    }

    private static ProcessStartInfo GetGitProcessStartInfo(string path, string arguments)
    {
        return new ProcessStartInfo
        {
            FileName = "git.exe",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = path
        };
    }

    private static IEnumerable<Branch> ParseBranchOutput(StreamReader standardOutput)
    {
        while (standardOutput.ReadLine() is { } line)
        {
            var split = line
                .Split("\t")
                .Select(text => text.Trim())
                .ToList();
            if (split is not [var head, var refName, var friendlyName, var upstreamTrack, var authorDateString, var author])
            {
                var message = $"""
                    The git branch result has not the expected format. Expected six blocks, separated by an tabulator,
                    but received {split.Count} blocks. The raw string to parse was:
                    {line}
                    """;
                throw new InvalidOperationException(message);
            }

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
