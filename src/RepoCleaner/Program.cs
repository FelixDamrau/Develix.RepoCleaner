using System.CommandLine;
using Develix.RepoCleaner.Model;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace Develix.RepoCleaner;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services
            .AddScoped<App>()
            .AddFluxor(o => o.ScanAssemblies(typeof(Program).Assembly));

        var customBinder = new ConsoleSettingsBinder();
        var rootCommand = ConsoleSettingsBinder.GetRootCommand();
        rootCommand.SetHandler((ConsoleSettings cs) => Run(cs, services), customBinder);
        return await rootCommand.InvokeAsync(args);
    }

    private static void Run(ConsoleSettings consoleSettings, IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var app = serviceProvider.GetRequiredService<App>();
        app.Run(consoleSettings);
    }
}
