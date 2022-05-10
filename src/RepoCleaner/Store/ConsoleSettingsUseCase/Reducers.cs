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
            Path = action.ConsoleArguments.Path,
            Pr = action.ConsoleArguments.Pr,
            AzureDevOpsUri = action.AppSettings.AzureDevopsOrgUri,
            ExcludedBranches = action.AppSettings.ExcludedBranches,
        };
    }

    [ReducerMethod(typeof(ConfigureCredentialsAction))]
    public static ConsoleSettingsState ExecuteConfigureCredentialsAction(ConsoleSettingsState state)
    {
        return state with { Configuring = true };
    }

    [ReducerMethod]
    public static ConsoleSettingsState ExecuteConfigureCredentialsResultAction(ConsoleSettingsState state, ConfigureCredentialsResultAction action)
    {
        return state with { Configuring = false, ConfigureResult = action.Result };
    }
}
