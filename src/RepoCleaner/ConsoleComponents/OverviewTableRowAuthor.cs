using System.Reflection;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

internal class OverviewTableRowAuthor : OverviewTableRowBase
{
    private readonly string author;
    private readonly List<PropertyInfo> columnPropertyInfos;
    private readonly OverviewTableRowBase parentRow;

    public OverviewTableRowAuthor(OverviewTableRowBase parentRow, string author)
    {
        this.author = author;
        columnPropertyInfos = GetStringPropertyInfos(parentRow.GetType()).Select(x => x.Property).ToList();
        this.parentRow = parentRow;
    }

    public override IEnumerable<string> GetColumns() => parentRow.GetColumns();

    public override IEnumerable<Markup> GetRowData()
    {
        var workItemTypeIndex = GetColumIndex(nameof(OverviewTableRow.WorkItemTypeString));
        var workItemTitleIndex = GetColumIndex(nameof(OverviewTableRow.Title));

        var authorColumns = Enumerable.Repeat(new Markup(string.Empty), columnPropertyInfos.Count).ToArray();
        authorColumns[workItemTypeIndex] = new Markup(":pencil:");
        authorColumns[workItemTitleIndex] = new Markup($"[grey30]{author}[/]");
        return authorColumns;
    }

    private int GetColumIndex(string columnpropertyName)
    {
        var columnPropertyInfo = columnPropertyInfos.Single(pi => pi.Name == columnpropertyName);
        return columnPropertyInfos.IndexOf(columnPropertyInfo);
    }
}
