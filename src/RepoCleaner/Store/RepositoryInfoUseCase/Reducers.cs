using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;

public static class Reducers
{
    [ReducerMethod]
    public static RepositoryInfoState SetRepository(RepositoryInfoState state, SetRepositoryAction action)
    {
        return state with { Repository = action.Repository };
    }

    [ReducerMethod]
    public static RepositoryInfoState SetWorkItems(RepositoryInfoState state, SetWorkItemsAction action)
    {
        return state with { WorkItems = action.WorkItems };
    }
}
