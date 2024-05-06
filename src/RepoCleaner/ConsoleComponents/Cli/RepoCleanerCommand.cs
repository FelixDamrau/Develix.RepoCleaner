using Develix.AzureDevOps.Connector.Model;
using Develix.AzureDevOps.Connector.Service;
using Develix.Essentials.Core;
using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;

internal class RepoCleanerCommand(
    AppSettings appSettings,
    IGitHandler gitHandler,
    IReposService reposService,
    IWorkItemService workItemService)
    : AsyncCommand<RepoCleanerSettings>
{
    private readonly AppSettings appSettings = appSettings;
    private readonly IGitHandler gitHandler = gitHandler;
    private readonly IReposService reposService = reposService;
    private readonly IWorkItemService workItemService = workItemService;

    public override async Task<int> ExecuteAsync(CommandContext context, RepoCleanerSettings settings)
    {
        var status = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Starting RepoCleaner", async ctx =>
        {
            ctx.Status("Logging in");
            var loginResult = await AzdoClient.Login(reposService, workItemService, settings, appSettings);
            if (!Validate(loginResult))
                return 1;

            ctx.Status("Getting repository");
            var repositoryResult = RepositoryInfo.Get(settings, appSettings, gitHandler);
            if (!Validate(repositoryResult))
                return 2;

            ctx.Status("Getting work items");
            var workItemsIds = RepositoryInfo.GetRelatedWorkItemIds(repositoryResult.Value);
            var workItemsResult = await workItemService.GetWorkItems(workItemsIds, settings.IncludePullRequests);
            if (!Validate(workItemsResult))
                return 3;

            ctx.Status("Rendering output");
            var table = new OverviewTable(appSettings, settings);
            var renderedTable = table.GetOverviewTable(workItemsResult.Value, repositoryResult.Value);
            AnsiConsole.Write(renderedTable);

            if (settings.Delete)
            {
                ctx.Status("Waiting for branches to delete");
                ExecuteDelete(repositoryResult.Value, workItemsResult.Value);
            }

            return 0;
        });
        return status;
    }

    private void ExecuteDelete(Repository repository, IEnumerable<WorkItem> workItems)
    {
        var branchesToDelete = Delete.Prompt(repository, workItems);
        Delete.Execute(gitHandler, repository, branchesToDelete);
    }

    private static bool Validate(IResult result)
    {
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
