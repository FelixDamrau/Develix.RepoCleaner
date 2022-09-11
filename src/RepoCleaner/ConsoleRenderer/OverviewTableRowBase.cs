using System.Reflection;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleRenderer;
public abstract class OverviewTableRowBase
{
    public IEnumerable<Markup> GetRowData()
    {
        return GetStringPropertyInfos()
            .OrderBy(x => x.Attribute.Order)
            .Select(x => GetMarkup(x.Property));
    }

    public IEnumerable<string> GetColumns()
    {
        return GetStringPropertyInfos()
            .OrderBy(x => x.Attribute.Order)
            .Select(a => a.Attribute.Title);
    }

    private IEnumerable<(PropertyInfo Property, OverviewTableColumnAttribute Attribute)> GetStringPropertyInfos()
    {
        return GetType()
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<OverviewTableColumnAttribute>()))
            .Where(x => x is (_, not null))
            .Select(x => (x.Property, x.Attribute!));
    }

    private Markup GetMarkup(PropertyInfo property)
    {
        var stringValue = (string?)property.GetValue(this);
        if (stringValue is null)
            throw new InvalidOperationException($"Could not get the value of string property '{property.Name}' of type '{GetType().Name}'!");

        return new(stringValue);
    }
}
