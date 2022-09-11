using System.Reflection;

namespace Develix.RepoCleaner.ConsoleRenderer;
public abstract class OverviewTableRowBase
{
    public string[] GetRowData()
    {
        return GetType()
            .GetProperties()
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<OverviewTableColumnAttribute>()))
            .Where(x => x is (not null, not null))
            .Select(x => (x.Property, x.Attribute!.Order))
            .OrderBy(x => x.Order)
            .Select(x => (string)x.Property.GetValue(this)!)
            .ToArray();
    }

    public string[] GetColumns()
    {
        return GetType()
            .GetProperties()
            .Select(p => p.GetCustomAttribute<OverviewTableColumnAttribute>())
            .Where(a => a is not null)
            .OrderBy(a => a!.Order)
            .Select(a => a!.Title)
            .ToArray();
    }
}
