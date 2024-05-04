using Develix.CredentialStore.Win32;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;

internal class ConfigCommand : Command
{
    public override int Execute(CommandContext context)
    {
        var prompt = new TextPrompt<string>("Enter [green]azure devops token[/]")
            .PromptStyle("red")
            .Secret();

        var token = AnsiConsole.Prompt(prompt);

        var credential = new Credential("token", token, AzdoClient.CredentialName);
        var crudResult = CredentialManager.CreateOrUpdate(credential);

        if (!crudResult.Valid)
        {
            var message = $"""
                Storing credentials failed.
                {crudResult.Message}";
                """;
            AnsiConsole.WriteLine(message);
            return 1;
        }
        return 0;
    }
}
