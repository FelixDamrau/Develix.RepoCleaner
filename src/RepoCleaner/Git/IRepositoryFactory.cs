using Develix.Essentials.Core;

namespace Develix.RepoCleaner.Git;

internal interface IRepositoryFactory
{
    Result<Model.Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches);
    Result<Model.Repository> GetRemoteRepository(string path, IEnumerable<string> excludedBranches);
    Result<Model.Repository> GetRepository(string path, IEnumerable<string> excludedBranches);
}
