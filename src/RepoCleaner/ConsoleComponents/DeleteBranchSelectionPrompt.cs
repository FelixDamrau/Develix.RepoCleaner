using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

public static class DeleteBranchSelectionPrompt
{
    public static IReadOnlyList<Branch> Show(IReadOnlyList<Branch> deletableBranches, IState<RepositoryInfoState> repositoryInfoState)
    {
        var instructionText =
            "[grey](Press [blue]<space>[/] to toggle deletion of a branch, " +
            "[green]<enter>[/] to delete selected branches.)[/]";
        var nonDeletableCount = repositoryInfoState.Value.Repository.Branches.Count - deletableBranches.Count;
        if (nonDeletableCount > 1) // current branch is never deletable
            instructionText += $"{Environment.NewLine}[grey]Remote braches cannot be deleted and are not shown here[/]";
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<Branch>()
                .UseConverter((b) => GetDisplayText(b, repositoryInfoState))
                .Title("Branches to delete?")
                .NotRequired()
                .PageSize(6)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .InstructionsText(instructionText)
                .AddChoices(deletableBranches));
    }

    private static string GetDisplayText(Branch branch, IState<RepositoryInfoState> repositoryInfoState)
    {
        var workItem = repositoryInfoState.Value.WorkItems.FirstOrDefault(wi => wi.Id == branch.RelatedWorkItemId);
        var displayText = workItem is null
            ? branch.FriendlyName
            : $"{branch.FriendlyName} [{workItem.Title}]";
        return displayText.EscapeMarkup();
    }
}
