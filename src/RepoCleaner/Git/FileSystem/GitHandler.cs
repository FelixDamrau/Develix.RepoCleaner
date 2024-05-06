using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;

namespace Develix.RepoCleaner.Git.FileSystem;

internal class GitHandler : IGitHandler
{
    private const string GitSubdirectory = @".git\";
    internal const string LocalBranchesPath = @"refs\heads\";
    internal const string RemoteBranchesPath = @"refs\remotes\origin\";

    public Result<Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repositoryProxy = GetRepository(path);
        return Result.Ok(repositoryProxy.ToRepository(BranchSourceKind.Local, excludedBranches));
    }

    public Result<Repository> GetRemoteRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repositoryProxy = GetRepository(path);
        return Result.Ok(repositoryProxy.ToRepository(BranchSourceKind.Remote, excludedBranches));
    }

    public Result<Repository> GetRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repositoryProxy = GetRepository(path);
        return Result.Ok(repositoryProxy.ToRepository(BranchSourceKind.All, excludedBranches));
    }

    public IReadOnlyList<Result> DeleteBranches(string repositoryPath, IEnumerable<Branch> branches)
    {
        return [Result.Fail($"The file system git handler does not support branch deletion")];
    }

    private static RepositoryProxy GetRepository(string path)
    {
        var localBranchNames = Directory.GetFiles(path, GitSubdirectory + LocalBranchesPath, SearchOption.AllDirectories);
        var remoteBranchNames = Directory.GetFiles(path, GitSubdirectory + RemoteBranchesPath, SearchOption.AllDirectories);
        var currentBranchName = GetCurrentBranchName(path);

        return new RepositoryProxy()
        {
            LocalBranchNames = localBranchNames,
            RemoteBranchNames = remoteBranchNames,
            CurrentBranchName = currentBranchName,
            Path = path,
        };
    }

    private static string GetCurrentBranchName(string path)
    {
        var headRow = File.ReadAllLines($@"{path}\{GitSubdirectory}\HEAD")[0];
        return headRow[0..5] == "ref: "
            ? GetBranchName(headRow) // a branch reference is checked out
            : headRow; // a tag is checked out or we're in a detached head state

        static string GetBranchName(string headRow)
        {
            return headRow[5..] // Get branch name
                .Replace("/", @"\"); // Hack windows file path...
        }
    }
}
