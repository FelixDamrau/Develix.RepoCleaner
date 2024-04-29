using System.Reflection;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

internal abstract class OverviewTableRowBase
{
    public virtual IEnumerable<Markup> GetRowData()
    {
        return GetStringPropertyInfos(GetType())
            .OrderBy(x => x.Attribute.Order)
            .Select(x => GetMarkup(x.Property));
    }

    public virtual IEnumerable<string> GetColumns()
    {
        return GetStringPropertyInfos(GetType())
            .OrderBy(x => x.Attribute.Order)
            .Select(a => a.Attribute.Title);
    }

    private protected static IEnumerable<(PropertyInfo Property, OverviewTableColumnAttribute Attribute)> GetStringPropertyInfos(Type type)
    {
        return type
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<OverviewTableColumnAttribute>()))
            .Where(x => x is (_, not null))
            .Select(x => (x.Property, x.Attribute!));
    }

    private Markup GetMarkup(PropertyInfo property)
    {
        var stringValue = (string?)property.GetValue(this)
            ?? throw new InvalidOperationException($"Could not get the value of string property '{property.Name}' of type '{GetType().Name}'!");
        try
        {
            return new(stringValue);
        }
        catch
        {
            return new($"[italic]{stringValue.EscapeMarkup()}[/]");
        }
    }
}
