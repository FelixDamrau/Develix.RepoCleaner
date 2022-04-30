using Develix.AzureDevOps.Connector.Model;

namespace Develix.RepoCleaner.Store;

public record SetRepositoryAction(Repository Repository);

public record SetWorkItemsAction(IReadOnlyList<WorkItem> WorkItems);
