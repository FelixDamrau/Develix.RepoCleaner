using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;

namespace Develix.RepoCleaner.Git.ExternalGit;
internal class RepositoryFactory : IRepositoryFactory
{
    private const string gitSubdirectory = @".git\";
    internal const string LocalBranchesPath = @"refs\heads\";
    internal const string RemoteBranchesPath = @"refs\remotes\origin\";

    public Result<Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repositoryProxy = GetRepository(path);
        return Result.Ok(repositoryProxy.ToRepository());
    }

    public Result<Repository> GetRemoteRepository(string path, IEnumerable<string> excludedBranches)
    {
        throw new NotImplementedException();
    }

    public Result<Repository> GetRepository(string path, IEnumerable<string> excludedBranches)
    {
        throw new NotImplementedException();
    }

    private RepositoryProxy GetRepository(string path)
    {
        var localBranchNames = Directory.GetFiles(path, gitSubdirectory + LocalBranchesPath, SearchOption.AllDirectories);
        var remoteBranchNames = Directory.GetFiles(path, gitSubdirectory + RemoteBranchesPath, SearchOption.AllDirectories);
        var currentBranch = ParseHeadFile(path);

        return new RepositoryProxy()
        {
            LocalBranchNames = localBranchNames,
            RemoteBranchNames = remoteBranchNames,
            CurrentBranchName = currentBranch,
        };
    }

    private string ParseHeadFile(string path)
    {
        var headRow = File.ReadAllLines(path + @"\" + gitSubdirectory + @"\HEAD")[0];
        return headRow[4..];
    }
}
