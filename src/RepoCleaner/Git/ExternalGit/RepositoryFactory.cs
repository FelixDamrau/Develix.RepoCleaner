using System.Runtime.CompilerServices;
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
        //return repositoryResult.Valid
        //    ? Result.Ok(Create(repositoryResult.Value, (b) => !b.IsRemote, excludedBranches))
        //    : Result.Fail<Model.Repository>(repositoryResult.Message);
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

    private IEnumerable<Branch> CreateBranches(string[] files)
    {
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var idString = GetId(fileName);
            var idValid = int.TryParse(idString, out var id) && id > 0;
            yield return new Branch()
            {
                FriendlyName = fileName,
                Name = fileName,
                RelatedWorkItemId = idValid ? id : null,
            };
        }
    }

    private static char[] GetId(string branchName)
    {
        return branchName
            .SkipWhile(c => !char.IsDigit(c))
            .TakeWhile(c => char.IsDigit(c))
            .ToArray();
    }
}
