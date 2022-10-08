using Develix.RepoCleaner.Git;
using Develix.RepoCleaner.Git.Model;
using Develix.RepoCleaner.Store.RepositoryInfoUseCase;
using Fluxor;
using Spectre.Console;

namespace Develix.RepoCleaner.ConsoleComponents;

public static class Delete
{
    public static IReadOnlyList<Branch> Prompt(IState<RepositoryInfoState> repositoryInfoState)
    {
        var deletableBranches = repositoryInfoState.Value.Repository.Branches.Where(b => IsDeletable(b)).ToList();

        if (deletableBranches.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey30]No branches can be deleted.[/]");
            return Array.Empty<Branch>();
        }
        return DeleteBranchSelectionPrompt.Show(deletableBranches, repositoryInfoState);

        static bool IsDeletable(Branch b) => !b.IsRemote && !b.IsCurrent;
    }

    public static void Execute(IReadOnlyCollection<Branch> branchesToDelete, IState<RepositoryInfoState> repositoryInfoState)
    {
        using var handler = new Handler(repositoryInfoState.Value.Repository);
        foreach (var branch in branchesToDelete)
        {
            var result = handler.TryDeleteBranch(branch);
            var message = result.Valid
                ? $"[green]Deleted branch {branch.FriendlyName}[/]"
                : $"[red]Failed:[/] {result.Message}";
            AnsiConsole.MarkupLine(message);
        }
    }
}
