using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace Develix.RepoCleaner.Git;

internal static class RepositoryFactory
{
    public static Model.Repository CreateLocal(Repository gitRepository, IEnumerable<string> excludedBranches)
        => Create(gitRepository, (b) => !b.IsRemote, excludedBranches);

    public static Model.Repository CreateRemote(Repository gitRepository, IEnumerable<string> excludedBranches)
        => Create(gitRepository, (b) => b.IsRemote, excludedBranches);

    public static Model.Repository CreateAll(Repository gitRepository, IEnumerable<string> excludedBranches)
        => Create(gitRepository, (b) => true, excludedBranches);

    private static Model.Repository Create(Repository gitRepository, Func<Branch, bool> selector, IEnumerable<string> excludedBranches)
    {
        var repository = new Model.Repository { Name = gitRepository.Info.WorkingDirectory };
        var regex = GetExcludedBranchesRegex(excludedBranches);

        foreach (var gitBranch in gitRepository.Branches.Where(b => selector(b) && !IsExcluded(b, regex)))
        {
            var branch = BranchFactory.Create(gitBranch);
            repository.AddBranch(branch);
        }
        return repository;
    }

    private static Regex GetExcludedBranchesRegex(IEnumerable<string> excludedBranches) => new($"(?:{string.Join('|', excludedBranches)})");

    private static bool IsExcluded(Branch branch, Regex excludedBranchesRegex) => excludedBranchesRegex.IsMatch(branch.FriendlyName);
}
