using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;

namespace Develix.RepoCleaner.Git.ExternalGit;
internal class RepositoryFactory : IRepositoryFactory
{
    public Result<Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repositoryResult = GetRepository(path);
        return Result.Ok(repositoryResult);
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

    private Repository GetRepository(string path)
    {
        var files = Directory.GetFiles(path, ".git\\refs\\heads\\");
        var branches = CreateBranches(files);
        var repository = new Repository("Hi", branches.First());
        foreach (var branch in branches)
        {
            repository.AddBranch(branch);
        }
        return repository;
    }

    private IEnumerable<Branch> CreateBranches(string[] files)
    {
        foreach ( var file in files)
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
