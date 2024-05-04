using Develix.AzureDevOps.Connector.Service;
using Develix.RepoCleaner.ConsoleComponents.Cli;
using Develix.RepoCleaner.ConsoleComponents.Cli.Infrastructure;
using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Model;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Develix.RepoCleaner;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        var registrar = InitRegistrar();
        var app = new CommandApp<RepoCleanerCommand>(registrar);
        app.Configure(configurator =>
        {
            configurator.AddCommand<ConfigCommand>("config");
        });
        return await app.RunAsync(args);
    }

    private static TypeRegistrar InitRegistrar()
    {
        var appSettings = AppSettings.Create();
        var registrations = new ServiceCollection();
        registrations.AddSingleton(appSettings);
        registrations.AddScoped<IWorkItemService, WorkItemService>();
        registrations.AddScoped<IReposService, ReposService>();
        registrations.AddScoped<IRepositoryFactory, Git.LibGit.RepositoryFactory> ();
        var registrar = new TypeRegistrar(registrations);
        return registrar;
    }
}
