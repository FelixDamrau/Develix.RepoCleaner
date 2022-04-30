using LibGit2Sharp;

namespace Develix.RepoCleaner.Git;

internal static class RepositoryFactory
{
    public static Model.Repository CreateLocal(Repository gitRepository) => Create(gitRepository, (b) => !b.IsRemote);

    public static Model.Repository CreateRemote(Repository gitRepository) => Create(gitRepository, (b) => b.IsRemote);

    public static Model.Repository CreateAll(Repository gitRepository) => Create(gitRepository, (b) => true);

    private static Model.Repository Create(Repository gitRepository, Func<Branch, bool> selector)
    {
        var repository = new Model.Repository
        {
            Name = gitRepository.Info.WorkingDirectory
        };
        foreach (var gitBranch in gitRepository.Branches.Where(b => selector(b)))
        {
            var branch = BranchFactory.Create(gitBranch);
            repository.AddBranch(branch);
        }

        return repository;
    }
}
