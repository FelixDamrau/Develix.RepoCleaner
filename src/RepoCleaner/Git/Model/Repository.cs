namespace Develix.RepoCleaner.Git.Model;

public class Repository
{
    public static readonly Repository DefaultInvalid = new("No repository loaded", new Branch());

    public string Name { get; } 

    public Branch CurrentBranch { get; }

    private readonly List<Branch> branches = new();

    public Repository(string name, Branch currentBranch)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        Name = name;
        CurrentBranch = currentBranch ?? throw new ArgumentNullException(nameof(currentBranch));
    }

    public IReadOnlyList<Branch> Branches => branches;

    public void AddBranch(Branch name) => branches.Add(name);
}
