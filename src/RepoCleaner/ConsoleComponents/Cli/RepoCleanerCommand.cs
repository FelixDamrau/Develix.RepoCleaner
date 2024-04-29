using Develix.AzureDevOps.Connector.Model;
using Develix.AzureDevOps.Connector.Service;
using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;

internal class RepoCleanerCommand : AsyncCommand<RepoCleanerSettings>
{
    private readonly AppSettings appSettings;
    private readonly IReposService reposService;
    private readonly IWorkItemService workItemService;

    public RepoCleanerCommand(
        AppSettings appSettings,
        IReposService reposService,
        IWorkItemService workItemService)
    {
        this.workItemService = workItemService;
        this.appSettings = appSettings;
        this.reposService = reposService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, RepoCleanerSettings settings)
    {
        var loginResult = await AzdoClient.Login(reposService, workItemService, settings, appSettings);
        if (!Validate(loginResult))
            return 1;

        var repositoryResult = RepositoryInfo.Get(settings, appSettings);
        if (!Validate(repositoryResult))
            return 2;

        var workItemsIds = RepositoryInfo.GetRelatedWorkItemIds(repositoryResult.Value);
        var workItemsResult = await workItemService.GetWorkItems(workItemsIds, settings.IncludePullRequests);
        if (!Validate(workItemsResult))
            return 3;

        var table = new OverviewTable(appSettings, settings);
        var renderedTable = table.GetOverviewTable(workItemsResult.Value, repositoryResult.Value);
        AnsiConsole.Write(renderedTable);

        if (settings.Delete)
            ExecuteDelete(repositoryResult.Value, workItemsResult.Value);

        return 0;
    }

    private static void ExecuteDelete(Repository repository, IEnumerable<WorkItem> workItems)
    {
        var x = Delete.Prompt(repository, workItems);
        Delete.Execute(repository, x);
    }

    private static bool Validate(IResult result) { 
        if (!result.Valid)
        {
            var message = $"""
                An error occurred.
                {result.Message}
                """;
            AnsiConsole.MarkupLine(message);
            return false;
        }
        return true;
    }
}
