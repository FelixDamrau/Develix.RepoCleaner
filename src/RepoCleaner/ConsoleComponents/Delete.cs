using Develix.AzureDevOps.Connector.Model;
using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

public static class Delete
{
    public static IReadOnlyList<Branch> Prompt(Repository repository, IEnumerable<WorkItem> workItems)
    {
        var deletableBranches = repository.Branches.Where(b => IsDeletable(b)).ToList();

        if (deletableBranches.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey30]No branches can be deleted.[/]");
            return [];
        }
        return Show(repository, workItems, deletableBranches);

        static bool IsDeletable(Branch b) => !b.IsRemote && !b.IsCurrent;
    }

    public static void Execute(Repository repository, IReadOnlyCollection<Branch> branchesToDelete)
    {
        using var handler = new Handler(repository);
        foreach (var branch in branchesToDelete)
        {
            var result = handler.TryDeleteBranch(branch);
            var message = result.Valid
                ? $"[green]Deleted branch {branch.FriendlyName}[/]"
                : $"[red]Failed:[/] {result.Message}";
            AnsiConsole.MarkupLine(message);
        }
    }

    private static List<Branch> Show(
        Repository repository,
        IEnumerable<WorkItem> workItems,
        IReadOnlyCollection<Branch> deletableBranches)
    {
        var instructionText =
            "[grey30](Press [blue]<space>[/] to toggle deletion of a branch, " +
            "[green]<enter>[/] to delete selected branches.)[/]";
        var nonDeletableCount = repository.Branches.Count - deletableBranches.Count;
        if (nonDeletableCount > 1) // current branch is never deletable
            instructionText += $"{Environment.NewLine}[grey30]Remote branches cannot be deleted and are not shown here[/]";
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<Branch>()
                .UseConverter((b) => GetDisplayText(b, workItems))
                .Title("Branches to delete?")
                .NotRequired()
                .PageSize(6)
                .MoreChoicesText("[grey30](Move up and down to reveal more branches)[/]")
                .InstructionsText(instructionText)
                .AddChoices(deletableBranches));
    }

    private static string GetDisplayText(Branch branch, IEnumerable<WorkItem> workItems)
    {
        var workItem = workItems.FirstOrDefault(wi => wi.Id == branch.RelatedWorkItemId);
        var displayText = workItem is null
            ? branch.FriendlyName
            : $"{branch.FriendlyName} [{workItem.Title}]";
        return displayText.EscapeMarkup();
    }
}
