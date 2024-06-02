using Develix.AzureDevOps.Connector.Service;
using Develix.CredentialStore.Win32;
using Develix.Essentials.Core;
using Develix.RepoCleaner.Model;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;

internal static class AzdoClient
{
    public const string CredentialName = "Develix:RepoCleanerAzureDevopsToken";

    public static async Task<Result> Login(
        IReposService reposService,
        IWorkItemService workItemService,
        RepoCleanerSettings settings,
        AppSettings appSettings)
    {
        var azureDevOpsOrganizationUri = new Uri(appSettings.AzureDevOpsOrganizationUri);
        IEnumerable<Task<Result>> loginActions = settings.IncludePullRequests
            ? [LoginWorkItemService(workItemService, azureDevOpsOrganizationUri), LoginReposService(reposService, azureDevOpsOrganizationUri)]
            : [LoginWorkItemService(workItemService, azureDevOpsOrganizationUri)];

        var loginResults = await Task.WhenAll(loginActions);
        if (loginResults.Where(r => !r.Valid).Select(r => r.Message).ToList() is { Count: >= 1 } errorMessages)
            return Result.Fail($"Login failed. {Environment.NewLine}{string.Join(Environment.NewLine, errorMessages)}");
        return Result.Ok();
    }

    private static async Task<Result> LoginWorkItemService(IWorkItemService workItemService, Uri azureDevopsUri)
    {
        var credential = CredentialManager.Get(CredentialName);
        return credential.Valid
            ? await workItemService.Initialize(azureDevopsUri, credential.Value.Password!)
            : Result.Fail(credential.Message);
    }

    private static async Task<Result> LoginReposService(IReposService reposService, Uri azureDevopsUri)
    {
        var credential = CredentialManager.Get(CredentialName);
        return credential.Valid
            ? await reposService.Initialize(azureDevopsUri, credential.Value.Password!)
            : Result.Fail(credential.Message);
    }
}
