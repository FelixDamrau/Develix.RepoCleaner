using Develix.AzureDevOps.Connector.Model;
using Develix.Essentials.Core;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Model;

namespace Develix.RepoCleaner.Store;

public record InitRepositoryAction(BranchSourceKind BranchSourceKind);

public record LoginServicesAction(string CredentialName);

public record LoginReposServiceAction(string CredentialName);

public record LoginReposServiceResultAction(Result LoginResult);

public record LoginWorkItemServiceAction(string CredentialName);

public record LoginWorkItemServiceResultAction(Result LoginResult);

public record SetRepositoryAction(Repository Repository);

public record SetWorkItemsAction(IReadOnlyList<WorkItem> WorkItems);

public record AddErrorAction(string Message);
