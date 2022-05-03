using Develix.CredentialStore.Win32;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Spectre.Console;

namespace Develix.RepoCleaner.Git;

public static class Reader
{
    private static readonly string path = Path.GetFullPath(@"c:\dev");

    public static Model.Repository GetLocalRepo(string path)
    {
        if (!Repository.IsValid(path))
            throw new GitHandlerException($"The provided path '{path}' does not contain a valid repository");

        var repository = new Repository(path);
        var remote = repository.Network.Remotes["origin"];

        if (remote is not null)
        {
            var credentials = GetCredentials(remote);
            if (CredentialsValid(remote, credentials, out var credentialsHandler))
            {

                var options = new FetchOptions
                {
                    Prune = true,
                    CredentialsProvider = credentialsHandler
                };
                repository.Network.Fetch(remote.Name, Enumerable.Empty<string>(), options);
            }
            else
            {
                throw new GitHandlerException($"No valid credentials could be generated.");
            }
        }
        return RepositoryFactory.CreateLocal(repository);
    }

    public static Model.Repository GetRemoteRepo(string path)
    {
        if (!Repository.IsValid(path))
            throw new GitHandlerException($"The provided path '{path}' does not contain a valid repository");

        var repository = new Repository(path);
        return RepositoryFactory.CreateRemote(repository);
    }

    public static Model.Repository GetRepo(string path)
    {
        var remote = GetRemoteRepo(path);
        var local = GetLocalRepo(path);

        foreach (var localBranch in local.Branches)
            remote.AddBranch(localBranch);

        return remote;
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

    private static bool CredentialsValid(Remote remote, Credentials credentials, out CredentialsHandler credentialsHandler)
    {
        credentialsHandler = new CredentialsHandler((_, _, _) => credentials);

        try
        {
            var remoteReferences = Repository.ListRemoteReferences(remote.Url, credentialsHandler);
            return remoteReferences.Any();
        }
        catch (LibGit2Sharp.LibGit2SharpException ex)
        {
            AnsiConsole.WriteLine($"The credentials seem to be invalid. Hops. Error message: {ex.Message}");
            return false;
        }
    }
}
