namespace Develix.RepoCleaner.Model;

public class ConsoleArguments
{
    public bool Delete { get; set; }
    public string? Path { get; init; }
    public BranchSourceKind Branches { get; init; }
    public bool Pr { get; init; }
    public bool Author { get; init; }
    public bool Config { get; init; }
}
