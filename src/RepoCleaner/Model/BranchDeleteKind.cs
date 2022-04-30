namespace Develix.RepoCleaner.Model;

/// <summary>
/// Describes the way this <see cref="RepoCleaner"/> will delete local branches.
/// </summary>
public enum BranchDeleteKind
{
    /// <summary>
    /// No branches will be deleted.
    /// </summary>
    None = 0,

    /// <summary>
    /// Prompt to delete all but excluded branches step by step.
    /// </summary>
    ByStep,

    /// <summary>
    /// Prompt to delete all branches step by step.
    /// </summary>
    ByStepAll,
}
