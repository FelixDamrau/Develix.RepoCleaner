using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using Develix.AzureDevOps.Connector.Service;
using Develix.RepoCleaner.Model;
using Fluxor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

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

        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHelp(ctx =>
            {
                ctx.HelpBuilder.CustomizeLayout(
                    _ =>
                        HelpBuilder.Default
                            .GetLayout()
                            .Skip(1)
                            .Prepend(
                                _ => AnsiConsole.Write(new CanvasImage("Images/unicorn.png"))
                            ));
            })
            .Build();

        return await parser.InvokeAsync(args);
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
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        if (Environment.GetEnvironmentVariable("DEV_ENVIRONMENT") is string env)
            configurationBuilder.AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: false);

        var configuration = configurationBuilder.Build();

        var appSettings = new AppSettings();
        var settingsSection = configuration.GetSection(AppSettings.SettingsSection);
        settingsSection.Bind(appSettings);
        if (appSettings.AzureDevopsOrgUri is null)
            throw new InvalidOperationException($"No azure devops uri is set. Please set a valid uri in appsettings.json.");

        return appSettings;
    }
}
