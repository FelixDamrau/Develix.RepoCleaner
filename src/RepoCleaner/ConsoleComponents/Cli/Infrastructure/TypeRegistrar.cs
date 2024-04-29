using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Develix.RepoCleaner.ConsoleComponents.Cli.Infrastructure;

public sealed class TypeRegistrar(IServiceCollection builder) : ITypeRegistrar
{
    private readonly IServiceCollection builder = builder;

    public ITypeResolver Build() => new TypeResolver(builder.BuildServiceProvider());

    public void Register(Type service, Type implementation)
    {
        _ = builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _ = builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        builder.AddSingleton(service, (provider) => func());
    }
}
