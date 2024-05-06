using Develix.Essentials.Core;

namespace Develix.RepoCleaner.Git;

internal interface IGitHandler
{
    Result<Model.Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches);
    Result<Model.Repository> GetRemoteRepository(string path, IEnumerable<string> excludedBranches);
    Result<Model.Repository> GetRepository(string path, IEnumerable<string> excludedBranches);
    IReadOnlyList<Result> DeleteBranches(string repositoryPath, IEnumerable<Model.Branch> branches);
}
