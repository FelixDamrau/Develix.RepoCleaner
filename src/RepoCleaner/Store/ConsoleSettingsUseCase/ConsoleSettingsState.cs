using Develix.RepoCleaner.Model;
using Fluxor;

namespace Develix.RepoCleaner.Store.ConsoleSettingsUseCase;

[FeatureState]
public record ConsoleSettingsState
{
    public bool Loading { get; init; }
    public string? Path { get; init; }
    public BranchDeleteKind Delete { get; init; }
    public BranchSourceKind Branches { get; init; }
    public bool Pr { get; init; }
    public bool Author { get; init; }
    public bool Config { get; init; }
    public Uri AzureDevOpsUri { get; init; } = null!;
    public List<string> ExcludedBranches { get; init; } = null!;
}
