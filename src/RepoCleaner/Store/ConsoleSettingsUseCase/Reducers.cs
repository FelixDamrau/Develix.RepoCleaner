using Fluxor;

namespace Develix.RepoCleaner.Store.ConsoleSettingsUseCase;

public static class Reducers
{
    [ReducerMethod]
    public static ConsoleSettingsState SetConsoleSettings(ConsoleSettingsState state, SetConsoleSettingsAction action)
    {
        return state with
        {
            Author = action.ConsoleArguments.Author,
            Branches = action.ConsoleArguments.Branches,
            Config = action.ConsoleArguments.Config,
            Delete = action.ConsoleArguments.Delete,
            Path = action.ConsoleArguments.Path,
            Pr = action.ConsoleArguments.Pr,
            AzureDevOpsUri = action.AppSettings.AzureDevopsOrgUri,
            ExcludedBranches = action.AppSettings.ExcludedBranches,
        };
    }
}
