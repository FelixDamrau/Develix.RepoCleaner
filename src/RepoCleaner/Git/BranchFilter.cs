using System.Text.RegularExpressions;
using Develix.RepoCleaner.Git.Model;

namespace Develix.RepoCleaner.Git;

internal static class BranchFilter
{
    public static IEnumerable<Branch> Filter(this IEnumerable<Branch> branches, IEnumerable<string> excludedBranches)
    {
        var regex = GetExcludedBranchesRegex(excludedBranches);
        return branches.Where(b => !IsExcluded(b.FriendlyName, regex));
    }

    public static Regex GetExcludedBranchesRegex(IEnumerable<string> excludedBranches)
    {
        return new Regex($"(?:{string.Join('|', excludedBranches)})");
    }

    public static bool IsExcluded(string branchName, Regex excludedBranchesRegex) => excludedBranchesRegex.IsMatch(branchName);
}
