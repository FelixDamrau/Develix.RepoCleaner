using Spectre.Console.Cli;

namespace Develix.RepoCleaner.ConsoleComponents.Cli.Infrastructure;
public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    private readonly IServiceProvider provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public object? Resolve(Type? type)
    {
        return type is null
            ? null
            : provider.GetService(type);
    }

    public void Dispose()
    {
        if (provider is IDisposable disposable)
            disposable.Dispose();
    }
}
