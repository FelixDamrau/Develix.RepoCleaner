namespace Develix.RepoCleaner.ConsoleComponents;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal class OverviewTableColumnAttribute(string title, int order) : Attribute
{
    public string Title { get; } = title ?? throw new ArgumentNullException(nameof(title));
    public int Order { get; } = order;
}
