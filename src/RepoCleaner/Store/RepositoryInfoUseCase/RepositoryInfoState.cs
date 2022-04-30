using Develix.AzureDevOps.Connector.Model;
using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;

[FeatureState]
public record RepositoryInfoState
{
    public Repository Repository { get; set; } = Repository.DefaultInvalid;

    public IReadOnlyList<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
