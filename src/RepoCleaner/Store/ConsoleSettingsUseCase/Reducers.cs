using Fluxor;

namespace Develix.RepoCleaner.Store.ConsoleSettingsUseCase;

public static class Reducers
{
    [ReducerMethod]
    public static ConsoleSettingsState SetConsoleSettings(ConsoleSettingsState state, SetConsoleSettingsAction action)
    {
        return state with
        {
            Author = action.ConsoleSettings.Author,
            Branches = action.ConsoleSettings.Branches,
            Config = action.ConsoleSettings.Config,
            Delete = action.ConsoleSettings.Delete,
            Path = action.ConsoleSettings.Path,
            Pr = action.ConsoleSettings.Pr,
            Show = action.ConsoleSettings.Show,
        };
    }
}
