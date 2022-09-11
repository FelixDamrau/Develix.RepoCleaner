namespace Develix.RepoCleaner.ConsoleRenderer;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class OverviewTableColumnAttribute : Attribute
{
    public string Title { get; }
    public int Order { get; }

    public OverviewTableColumnAttribute(string title, int order)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Order = order;
    }
}
