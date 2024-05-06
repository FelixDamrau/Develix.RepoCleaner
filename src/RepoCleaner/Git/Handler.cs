using Develix.Essentials.Core;
using LibGit2Sharp;

namespace Develix.RepoCleaner.Git;

public sealed class Handler : IDisposable
{
    private readonly Repository gitRepository;

    public Handler(Model.Repository repository)
    {
        var path = repository.Path;
        if (!Repository.IsValid(path))
            throw new ArgumentException($"The path '{path}' does not point to a valid git repository.", path);

        gitRepository = new Repository(path);
    }

    public Result TryDeleteBranch(Model.Branch branch)
    {
        if (gitRepository.Branches[branch.FriendlyName] is not Branch gitBranch)
            return Result.Fail($"Deletion failed, branch '{branch.FriendlyName}' was not found.");

        try
        {
            gitRepository.Branches.Remove(gitBranch);
        }
        catch (LibGit2SharpException ex)
        {
            return Result.Fail($"Deleting the branch failed horribly. Did you try to delete the current head? (Exception message is: {ex.Message})");
        }
        return Result.Ok();
    }

    #region IDisposable
    private bool disposedValue;
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
                gitRepository.Dispose();

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
