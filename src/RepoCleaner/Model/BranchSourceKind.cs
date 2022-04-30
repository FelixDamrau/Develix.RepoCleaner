namespace Develix.RepoCleaner.Model;

/// <summary>
/// Describes the source from where this <see cref="RepoCleaner"/> will load branches.
/// </summary>
[Flags]
public enum BranchSourceKind
{
    /// <summary>
    /// Show only local branches
    /// </summary>
    Local = 1 << 1,

    /// <summary>
    /// Show only remote branches
    /// </summary>
    Remote = 1 << 2,

    /// <summary>
    /// Show remote and local branches
    /// </summary>
    All = Local | Remote
}
