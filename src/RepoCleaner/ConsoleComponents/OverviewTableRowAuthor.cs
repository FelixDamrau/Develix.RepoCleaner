namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTableRowAuthor : OverviewTableRowCustomBase
{
    public OverviewTableRowAuthor(OverviewTableRowBase parentRow, string author)
        : base(parentRow, ":pencil:", author)
    {
    }
}
