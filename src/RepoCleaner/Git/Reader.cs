using Develix.CredentialStore.Win32;
using Develix.Essentials.Core;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Spectre.Console;

namespace Develix.RepoCleaner.Git;

public static class Reader
{
    public static Result<Model.Repository> GetLocalRepo(string path)
    {
        var repositoryResult = GetRepository(path);

        return repositoryResult.Valid
            ? Result.Ok(RepositoryFactory.CreateLocal(repositoryResult.Value))
            : Result.Fail<Model.Repository>(repositoryResult.Message);
    }

    public static Result<Model.Repository> GetRemoteRepo(string path)
    {
        var repositoryResult = GetRepository(path);

        return repositoryResult.Valid
            ? Result.Ok(RepositoryFactory.CreateRemote(repositoryResult.Value))
            : Result.Fail<Model.Repository>(repositoryResult.Message);
    }

    public static Result<Model.Repository> GetRepo(string path)
    {
        var remoteResult = GetRemoteRepo(path);
        var localResult = GetLocalRepo(path);

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
                repository.Network.Fetch(remote.Name, Enumerable.Empty<string>(), options);
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
        var gitRepositoryHost = DeterminateHostKind(remote.Url);
        return GetCredentials(gitRepositoryHost);
    }

    private static GitRepositoryHost DeterminateHostKind(string url)
    {
        return url switch
        {
            string s when s.StartsWith("https://github.com") => GitRepositoryHost.GitHub,
            string s when s.StartsWith("https://dev.azure.com") => GitRepositoryHost.AzureDevops,
            _ => GitRepositoryHost.Unknown
        };
    }

    private static Credentials GetCredentials(GitRepositoryHost gitRepositoryHost)
    {
        const string gitHubIdentifier = "git:https://github.com";
        const string azureDevopsIdentifier = "git:https://dev.azure.com";

        return gitRepositoryHost switch
        {
            GitRepositoryHost.AzureDevops => GetCredentials(azureDevopsIdentifier),
            GitRepositoryHost.GitHub => GetCredentials(gitHubIdentifier),
            GitRepositoryHost.Invalid or GitRepositoryHost.Unknown => new DefaultCredentials(),
            _ => throw new GitHandlerException($"The {nameof(GitRepositoryHost)} '{gitRepositoryHost}' is not implemented yet. Whops.)")
        };
    }

    private static Credentials GetCredentials(string gitRepositoryHostIdentifier)
    {
        var credentialResult = CredentialManager.Get(gitRepositoryHostIdentifier);
        if (!credentialResult.Valid)
        {
            AnsiConsole.WriteLine($"Could not find any windows credential for {gitRepositoryHostIdentifier}. Using default credentials...");
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
}
