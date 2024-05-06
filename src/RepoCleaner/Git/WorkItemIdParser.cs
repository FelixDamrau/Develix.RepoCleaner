namespace Develix.RepoCleaner.Git;

internal static class WorkItemIdParser
{
    public static int? Parse(string value)
    {
        var idString = GetId(value);
        return int.TryParse(idString, out var workItemId) && workItemId > 0
            ? workItemId
            : null;
    }

    private static ReadOnlySpan<char> GetId(string branchName)
    {
        return branchName
            .SkipWhile(c => !char.IsDigit(c))
            .TakeWhile(c => char.IsDigit(c))
            .ToArray();
    }
}
