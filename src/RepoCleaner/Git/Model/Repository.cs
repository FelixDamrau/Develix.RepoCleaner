namespace Develix.RepoCleaner.Git.Model;

public class Repository
{
    public static readonly Repository DefaultInvalid = new("No repository loaded", new Branch());

    private readonly List<Branch> branches = [];

    public Repository(string name, Branch currentBranch)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' of a {nameof(Repository)} cannot be null or empty.", nameof(name));

        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<Branch> Branches => branches;

    public void AddBranch(Branch name) => branches.Add(name);
}
