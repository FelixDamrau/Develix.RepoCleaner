using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;
public static class DeleteBranchSelectionPrompt
{
    public static IReadOnlyList<Branch> Show(IReadOnlyList<Branch> deletableBranches, RepositoryInfoState repositoryInfoState)
    {
        var instructionText =
            "[grey](Press [blue]<space>[/] to toggle deletion of a branch, " +
            "[green]<enter>[/] to delete selected branches.)[/]";
        var nonDeletableCount = repositoryInfoState.Repository.Branches.Count - deletableBranches.Count;
        if (nonDeletableCount > 1) // current branch is never deletable
            instructionText += $"{Environment.NewLine}[grey]Remote braches cannot be deleted and are not shown here[/]";
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<Branch>()
                .UseConverter((b) => GetDisplayText(b))
                .Title("Branches to delete?")
                .NotRequired()
                .PageSize(6)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .InstructionsText(instructionText)
                .AddChoices(deletableBranches));

        bool IsDeletable(Branch b) => !b.IsRemote && !b.IsCurrent;
        string GetDisplayText(Branch branch)
        {
            var workItem = repositoryInfoState.WorkItems.FirstOrDefault(wi => wi.Id == branch.RelatedWorkItemId);
            var displayText = workItem is null
                ? branch.FriendlyName
                : $"{branch.FriendlyName} [{workItem.Title}]";
            return displayText.EscapeMarkup();
        }
    }
}
