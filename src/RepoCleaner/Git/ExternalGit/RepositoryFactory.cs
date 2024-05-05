using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;

namespace Develix.RepoCleaner.Git.ExternalGit;
internal class RepositoryFactory : IRepositoryFactory
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

    private static RepositoryProxy GetRepository(string path)
    {
        var localBranchNames = Directory.GetFiles(path, GitSubdirectory + LocalBranchesPath, SearchOption.AllDirectories);
        var remoteBranchNames = Directory.GetFiles(path, GitSubdirectory + RemoteBranchesPath, SearchOption.AllDirectories);
        var currentBranch = ParseHeadFile(path);

        return new RepositoryProxy()
        {
            LocalBranchNames = localBranchNames,
            RemoteBranchNames = remoteBranchNames,
            CurrentBranchName = currentBranch,
        };
    }

    private static string ParseHeadFile(string path)
    {
        var headRow = File.ReadAllLines(path + @"\" + GitSubdirectory + @"\HEAD")[0];
        var branchName = headRow[5..];
        return branchName.Replace("/", @"\"); // Hack windows file path...
    }
}
