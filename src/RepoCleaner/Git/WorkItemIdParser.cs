using Develix.RepoCleaner.Git.Model;

namespace Develix.RepoCleaner.Git;

internal static class WorkItemIdParser
{
    public static bool TryParse(string value, out int workItemId)
    {
        var idString = GetId(value);
        return int.TryParse(idString, out workItemId) && workItemId > 0;
    }

    private static ReadOnlySpan<char> GetId(string branchName)
    {
        return branchName
            .SkipWhile(c => !char.IsDigit(c))
            .TakeWhile(c => char.IsDigit(c))
            .ToArray();
    }
}
