namespace Develix.RepoCleaner.Git.Model;

public class Repository
{
    private readonly List<Branch> branches = [];

    public Repository(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException($"'{nameof(path)}' of a {nameof(Repository)} cannot be null or empty.", nameof(path));

        Path = path;
    }

    public string Path { get; }

    public IReadOnlyList<Branch> Branches => branches;

    public void AddBranch(Branch name) => branches.Add(name);
}
