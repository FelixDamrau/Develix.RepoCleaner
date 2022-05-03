using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;

namespace Develix.RepoCleaner.Store;

public record InitRepositoryAction();

public record SetRepositoryAction(Repository Repository);

public record SetWorkItemsAction(IReadOnlyList<WorkItem> WorkItems);
