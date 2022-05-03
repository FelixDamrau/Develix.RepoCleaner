using System.CommandLine;
using Develix.AzureDevOps.Connector.Service;
using Develix.RepoCleaner.Model;
using Fluxor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Develix.RepoCleaner;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddScoped<App>();
        services.AddFluxor(o => o.ScanAssemblies(typeof(Program).Assembly));
        services.AddScoped<IWorkItemService, WorkItemService>();
        services.AddScoped<IReposService, ReposService>();

        var customBinder = new ConsoleArgumentsBinder();
        var rootCommand = ConsoleArgumentsBinder.GetRootCommand();
        rootCommand.SetHandler((ConsoleArguments cs) => Run(cs, services), customBinder);
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task Run(ConsoleArguments consoleArguments, IServiceCollection services)
    {
        var appSettings = GetAppSettings();
        var serviceProvider = services.BuildServiceProvider();
        var app = serviceProvider.GetRequiredService<App>();
        await app.Run(consoleArguments, appSettings);
    }

    private static AppSettings GetAppSettings()
    {
        var env = Environment.GetEnvironmentVariable("DEV_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .Build();

        var appSettings = new AppSettings();
        var settingsSection = configuration.GetSection(AppSettings.SettingsSection);
        settingsSection.Bind(appSettings);
        if (appSettings.AzureDevopsOrgUri is null)
            throw new InvalidOperationException($"No azure devops uri is set. Please set a valid uri in appsettings.json.");

        return appSettings;
    }
}
