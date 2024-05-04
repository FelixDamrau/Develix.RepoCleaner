using System.Text.RegularExpressions;
using Develix.CredentialStore.Win32;
using Develix.Essentials.Core;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Spectre.Console;

namespace Develix.RepoCleaner.Git.LibGit;

internal class RepositoryFactory : IRepositoryFactory
{
    public Result<Model.Repository> GetLocalRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repositoryResult = GetRepository(path);

        return repositoryResult.Valid
            ? Result.Ok(Create(repositoryResult.Value, (b) => !b.IsRemote, excludedBranches))
            : Result.Fail<Model.Repository>(repositoryResult.Message);
    }

    public Result<Model.Repository> GetRemoteRepository(string path, IEnumerable<string> excludedBranches)
    {
        var repositoryResult = GetRepository(path);

        return repositoryResult.Valid
            ? Result.Ok(Create(repositoryResult.Value, (b) => b.IsRemote, excludedBranches))
            : Result.Fail<Model.Repository>(repositoryResult.Message);
    }

    public Result<Model.Repository> GetRepository(string path, IEnumerable<string> excludedBranches)
    {
        var remoteResult = GetRemoteRepository(path, excludedBranches);
        var localResult = GetLocalRepository(path, excludedBranches);

        if (!remoteResult.Valid)
            return Result.Fail<Model.Repository>(remoteResult.Message);
        if (!localResult.Valid)
            return Result.Fail<Model.Repository>(localResult.Message);

        foreach (var localBranch in localResult.Value.Branches)
            remoteResult.Value.AddBranch(localBranch);

        return Result.Ok(remoteResult.Value);
    }

    private static Result<Repository> GetRepository(string path)
    {
        if (!Repository.IsValid(path))
            return Result.Fail<Repository>($"The provided path '{path}' does not contain a valid repository");

        var repository = new Repository(path);
        var remote = repository.Network.Remotes["origin"];

        if (remote is not null)
        {
            var credentials = GetCredentials(remote);
            var handlerResult = GetCredentialsHandler(remote, credentials);
            if (handlerResult.Valid)
            {
                var options = new FetchOptions
                {
                    Prune = true,
                    CredentialsProvider = handlerResult.Value,
                };
                repository.Network.Fetch(remote.Name, [], options);
            }
            else
            {
                return Result.Fail<Repository>($"No valid credentials could be generated. Error message: {handlerResult.Message}");
            }
        }

        return Result.Ok(repository);
    }

    private static Credentials GetCredentials(Remote remote)
    {
        return GetGitCredentials(new Uri(remote.Url));
    }

    private static Credentials GetGitCredentials(Uri url)
    {
        const string azureDevopsIdentifier = "git:https://dev.azure.com";
        const string gitHubIdentifier = "git:https://github.com";

        return url switch
        {
            { Authority: "dev.azure.com", UserInfo: "" or null } => GetCredentials(azureDevopsIdentifier),
            { Authority: "dev.azure.com" } => GetCredentials($"{azureDevopsIdentifier}/{url.UserInfo}"),
            { Authority: "github.com" } => GetCredentials(gitHubIdentifier),
            _ => new DefaultCredentials(),
        };
    }

    private static Credentials GetCredentials(string gitCredentialName)
    {
        var credentialResult = CredentialManager.Get(gitCredentialName);
        if (!credentialResult.Valid)
        {
            AnsiConsole.WriteLine($"Could not find any windows credential for {gitCredentialName}. Using default credentials...");
            AnsiConsole.WriteLine($"Error message: {credentialResult.Message}");
            return new DefaultCredentials();
        }
        return new UsernamePasswordCredentials
        {
            Username = credentialResult.Value.UserName,
            Password = credentialResult.Value.Password
        };
    }

    private static Result<CredentialsHandler> GetCredentialsHandler(Remote remote, Credentials credentials)
    {
        var credentialsHandler = new CredentialsHandler((_, _, _) => credentials);

        try
        {
            var remoteReferences = Repository.ListRemoteReferences(remote.Url, credentialsHandler);
            return remoteReferences.Any()
                ? Result.Ok(credentialsHandler)
                : Result.Fail<CredentialsHandler>("No remote references were found!");
        }
        catch (LibGit2SharpException ex)
        {
            return Result.Fail<CredentialsHandler>($"The credentials could not be verified. Exception: {ex.Message}");
        }
    }

    private static Model.Repository Create(Repository gitRepository, Func<Branch, bool> selector, IEnumerable<string> excludedBranches)
    {
        var repository = new Model.Repository(gitRepository.Info.WorkingDirectory, BranchFactory.Create(gitRepository.Head));
        var regex = GetExcludedBranchesRegex(excludedBranches);

        foreach (var gitBranch in gitRepository.Branches.Where(b => selector(b) && !IsExcluded(b, regex)))
        {
            var branch = BranchFactory.Create(gitBranch);
            repository.AddBranch(branch);
        }
        return repository;
    }

    private static Regex GetExcludedBranchesRegex(IEnumerable<string> excludedBranches) => new($"(?:{string.Join('|', excludedBranches)})");

    private static bool IsExcluded(Branch branch, Regex excludedBranchesRegex) => excludedBranchesRegex.IsMatch(branch.FriendlyName);
}
