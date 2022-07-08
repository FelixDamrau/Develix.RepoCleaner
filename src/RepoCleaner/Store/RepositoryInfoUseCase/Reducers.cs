using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;

public static class Reducers
{
    [ReducerMethod(typeof(InitRepositoryAction))]
    public static RepositoryInfoState InitRepository(RepositoryInfoState state)
    {
        return state with { WorkItemsLoaded = false, RepositoryLoaded = false };
    }

    [ReducerMethod(typeof(LoginServicesAction))]
    public static RepositoryInfoState Login(RepositoryInfoState state)
    {
        return state with { };
    }

    [ReducerMethod(typeof(LoginReposServiceAction))]
    public static RepositoryInfoState LoginReposService(RepositoryInfoState state)
    {
        return state with { ReposServiceConnected = false };
    }

    [ReducerMethod]
    public static RepositoryInfoState LoginReposService(RepositoryInfoState state, LoginReposServiceResultAction action)
    {
        return state with { ReposServiceConnected = action.LoginResult.Valid };
    }

    [ReducerMethod(typeof(LoginWorkItemServiceAction))]
    public static RepositoryInfoState LoginWorkItemService(RepositoryInfoState state)
    {
        return state with { WorkItemServiceState = ServiceConnectionState.Connecting };
    }

    [ReducerMethod]
    public static RepositoryInfoState LoginRepoService(RepositoryInfoState state, LoginWorkItemServiceResultAction action)
    {
        var serviceState = action.LoginResult.Valid ? ServiceConnectionState.Connected : ServiceConnectionState.FailedToConnect;
        return state with { WorkItemServiceState = serviceState };
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

    [ReducerMethod]
    public static RepositoryInfoState AddErrorMessage(RepositoryInfoState state, AddErrorAction action)
    {
        return state with { ErrorMessages = state.ErrorMessages.Append(action.Message).ToList() };
    }
}
