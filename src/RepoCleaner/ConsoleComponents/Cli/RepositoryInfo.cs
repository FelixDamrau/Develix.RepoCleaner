using Develix.Essentials.Core;
using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;

internal static class RepositoryInfo
{
    public static Result<Repository> Get(RepoCleanerSettings settings, AppSettings appSettings, IRepositoryFactory repositoryFactory)
    {
        var path = settings.Path ?? Directory.GetCurrentDirectory();
        return settings.BranchSource switch
        {
            BranchSourceKind.Local => repositoryFactory.GetLocalRepository(path, appSettings.ExcludedBranches),
            BranchSourceKind.Remote => repositoryFactory.GetRemoteRepository(path, appSettings.ExcludedBranches),
            BranchSourceKind.All => repositoryFactory.GetRepository(path, appSettings.ExcludedBranches),
            _ => throw new NotSupportedException($"The {nameof(BranchSourceKind)} '{settings.BranchSource}' is not supported!"),
        };
    }

    public static IReadOnlyCollection<int> GetRelatedWorkItemIds(Repository repository)
    {
        return repository.Branches.Select(b => b.RelatedWorkItemId).OfType<int>().ToList();
    }
}
