﻿using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git.Model;
using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;

[FeatureState]
public record RepositoryInfoState
{
    public bool RepositoryLoaded { get; init; }
    public bool WorkItemsLoaded { get; init; }
    public Repository Repository { get; init; } = Repository.DefaultInvalid;

    public IReadOnlyList<WorkItem> WorkItems { get; init; } = new List<WorkItem>();
}
