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
            configurator.PropagateExceptions();
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
        AddRepositoryFactory(registrations, appSettings);
        var registrar = new TypeRegistrar(registrations);
        return registrar;
    }

    private static void AddRepositoryFactory(ServiceCollection registrations, AppSettings appSettings)
    {
        if (appSettings.GitHandler == GitHandlerKind.LibGit2Sharp)
            registrations.AddScoped<IGitHandler, Git.LibGit.GitHandler>();
        else if (appSettings.GitHandler == GitHandlerKind.FileSystem)
            registrations.AddScoped<IGitHandler, Git.FileSystem.GitHandler>();
        else if(appSettings.GitHandler == GitHandlerKind.External)
            registrations.AddScoped<IGitHandler, Git.External.GitHandler>();
        else
            throw new NotSupportedException($"The {nameof(GitHandlerKind)} of type {appSettings.GitHandler} is not supported yet!");
    }
}
