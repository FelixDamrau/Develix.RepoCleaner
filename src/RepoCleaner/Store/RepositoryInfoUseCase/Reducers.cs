using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;

public static class Reducers
{
    public static RepositoryInfoState InitRepository(RepositoryInfoState state, InitRepositoryAction action)
    {
        return state with { WorkItemsLoaded = false, RepositoryLoaded = false };
    }

    [ReducerMethod]
    public static RepositoryInfoState SetRepository(RepositoryInfoState state, SetRepositoryAction action)
    {
        return state with { Repository = action.Repository, RepositoryLoaded = true };
    }

    [ReducerMethod]
    public static RepositoryInfoState SetWorkItems(RepositoryInfoState state, SetWorkItemsAction action)
    {
        return state with { WorkItems = action.WorkItems, WorkItemsLoaded = true };
    }
}
