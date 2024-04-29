using Develix.RepoCleaner.Model;
using Spectre.Console.Cli;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;

internal class RepoCleanerSettings : CommandSettings
{
    [CommandOption("-d|--delete")]
    public bool Delete { get; set; }

    [CommandOption("-p|--path <PATH>")]
    public string? Path { get; set; }

    [CommandOption("-b|--branch <BRANCH_SOURCE_KIND>")]
    public BranchSourceKind BranchSource { get; set; } = BranchSourceKind.Local;

    [CommandOption("--pr|--pull-requests")]
    public bool IncludePullRequests { get; set; }

    [CommandOption("--author")]
    public bool IncludeAuthor { get; set; }
}
