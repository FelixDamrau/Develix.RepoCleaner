using System.Reflection;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

internal abstract class OverviewTableRowCustomBase : OverviewTableRowBase
{
    protected readonly PropertyInfo[] columnPropertyInfos;
    protected readonly OverviewTableRowBase parentRow;

    protected string Icon { get; }
    protected string Data { get; }

    public OverviewTableRowCustomBase(OverviewTableRowBase parentRow, string icon, string data)
    {
        columnPropertyInfos = GetStringPropertyInfos(parentRow.GetType()).Select(x => x.Property).ToArray();
        this.parentRow = parentRow;
        Icon = icon;
        Data = data;
    }

    public override IEnumerable<string> GetColumns() => parentRow.GetColumns();

    public override IEnumerable<Markup> GetRowData()
    {
        var workItemTypeIndex = GetColumIndex(nameof(OverviewTableRow.WorkItemTypeString));
        var titleIndex = GetColumIndex(nameof(OverviewTableRow.Title));

        var columns = Enumerable.Repeat(new Markup(string.Empty), columnPropertyInfos.Length).ToArray();
        columns[workItemTypeIndex] = new Markup(Icon);
        columns[titleIndex] = new Markup($"[grey30]{Data}[/]");
        return columns;
    }

    protected int GetColumIndex(string columnpropertyName)
    {
        var columnPropertyInfo = columnPropertyInfos.Single(pi => pi.Name == columnpropertyName);
        return Array.IndexOf(columnPropertyInfos, columnPropertyInfo);
    }

}
