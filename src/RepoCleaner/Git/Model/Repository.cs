namespace Develix.RepoCleaner.Git.Model;

public class Repository
{
    public static readonly Repository DefaultInvalid = new Repository { Name = "No repository loaded" };

    private string name = "Name unknown";
    public string Name
    {
        get { return name; }
        init { name = value ?? throw new ArgumentNullException(nameof(Name)); }
    }

    private readonly List<Branch> branches = new();
    public IReadOnlyList<Branch> Branches => branches;

    public void AddBranch(Branch name) => branches.Add(name);
}
