using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;

[FeatureState]
public record RepositoryInfoState
{
    public bool RepositoryLoaded { get; init; }
    public bool WorkItemsLoaded { get; init; }
    public bool ReposServiceConnected { get; init; }
    public ServiceConnectionState WorkItemServiceState { get; init; } = ServiceConnectionState.Disconnected;
    public Repository Repository { get; init; } = Repository.DefaultInvalid;

    public IReadOnlyList<WorkItem> WorkItems { get; init; } = new List<WorkItem>();
}
