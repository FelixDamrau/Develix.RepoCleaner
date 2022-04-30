namespace Develix.RepoCleaner.Model;
public class ConsoleSettings
{
    public bool Show { get; init; }
    public string? Path { get; init; }
    public BranchDeleteKind Delete { get; init; }
    public BranchSourceKind Branches { get; init; }
    public bool Pr { get; init; }
    public bool Author { get; init; }
    public bool Config { get; init; }
}
