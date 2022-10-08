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
            AzureDevOpsUri = action.AppSettings.AzureDevopsOrgUri,
            Branches = action.ConsoleArguments.Branches,
            Config = action.ConsoleArguments.Config,
            ExcludedBranches = action.AppSettings.ExcludedBranches,
            ShortProjectNames = new Dictionary<string, string>(action.AppSettings.ShortProjectNames, StringComparer.OrdinalIgnoreCase),
            WorkItemTypeIcons = new Dictionary<string, string>(action.AppSettings.WorkItemTypeIcons, StringComparer.OrdinalIgnoreCase),
            Path = action.ConsoleArguments.Path,
            Pr = action.ConsoleArguments.Pr,
            ShowDeletePrompt = action.ConsoleArguments.Delete,
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
