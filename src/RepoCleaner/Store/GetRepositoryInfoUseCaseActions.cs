using Develix.AzureDevOps.Connector.Model;
using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;

namespace Develix.RepoCleaner.Store;

public record InitRepositoryAction();

public record LoginServicesAction(string CredentialName);

public record LoginReposServiceAction(string CredentialName);

public record LoginReposServiceResultAction(Result loginResult);

public record LoginWorkItemServiceAction(string CredentialName);

public record LoginWorkItemServiceResultAction(Result loginResult);

public record SetRepositoryAction(Repository Repository);

public record SetWorkItemsAction(IReadOnlyList<WorkItem> WorkItems);
