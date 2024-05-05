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
        AddRepositoryFactory(registrations, appSettings);
        var registrar = new TypeRegistrar(registrations);
        return registrar;
    }

    private static void AddRepositoryFactory(ServiceCollection registrations, AppSettings appSettings)
    {
        if (appSettings.GitHandler is GitHandlerKind.Default or GitHandlerKind.Lib2GitSharp)
            registrations.AddScoped<IRepositoryFactory, Git.LibGit.RepositoryFactory>();
        else if (appSettings.GitHandler is GitHandlerKind.File)
            registrations.AddScoped<IRepositoryFactory, Git.ExternalGit.RepositoryFactory>();
        else
            throw new NotSupportedException($"The {nameof(GitHandlerKind)} of type {appSettings.GitHandler} is not supported yet!");
    }
}
