using Develix.RepoCleaner.Store;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Develix.RepoCleaner.Utils;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;
public static class Config
{
    public static void Show(string credentialName, IState<ConsoleSettingsState> consoleSettingsState, IDispatcher dispatcher)
    {
        var token = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]azure devops token[/]")
            .PromptStyle("red")
            .Secret());
        AnsiConsole
            .Status()
            .Start("Storing credentials",
                async (ctx) =>
                {
                    var action = new ConfigureCredentialsAction(credentialName, token);
                    dispatcher.Dispatch(action);
                    await AsyncHelper.WaitUntilAsync(() => !consoleSettingsState.Value.Configuring, 100, 2000, default);
                    ctx.Status("Credentials initialized");
                });
    }
}
