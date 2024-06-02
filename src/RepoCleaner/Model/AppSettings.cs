using YamlDotNet.Serialization;

namespace Develix.RepoCleaner.Model;

public class AppSettings
{
    [YamlMember(Description = "The URI of the Azure DevOps organization, e.g.: https://dev.azure.com/myAzureDevOpsOrganization/")]
    public string AzureDevOpsOrganizationUri { get; set; } = "https://dev.azure.com/myAzureDevOpsOrganization/";

    [YamlMember(Description = "A collection of regular expressions that are used to exclude branches based on their names.")]
    public List<string> ExcludedBranches { get; set; } = ["^release", "^main$", "^master$"];

    [YamlMember(Description = $"Determines the type of Git handler to  use. [{nameof(GitHandlerKind.External)}, {nameof(GitHandlerKind.LibGit2Sharp)} or {nameof(GitHandlerKind.FileSystem)}]")]
    public GitHandlerKind GitHandler { get; set; } = GitHandlerKind.External;

    [YamlMember(Description = "A mapping of long project names to short names for shorter table headers in the output.")]
    public Dictionary<string, string> ShortProjectNames { get; set; } = new()
    {
        ["LongProjectName"] = "LPN",
    };

    [YamlMember(Description = "A mapping of Azure DevOps work item types to their respective icons.")]
    public Dictionary<string, string> WorkItemTypeIcons { get; set; } = new()
    {
        ["Bug"] = ":lady_beetle:",
        ["Epic"] = ":star:",
        ["Feature"] = ":trophy:",
        ["Impediment"] = ":red_triangle_pointed_up:",
        ["Product Backlog Item"] = ":notebook:",
        ["Task"] = ":spiral_notepad:",
        ["Requirement"] = ":page_facing_up:",
        ["Test Case"] = ":alembic:",
        ["Review"] = ":left_speech_bubble:",
        ["Issue"] = ":red_triangle_pointed_up:",
        ["Change Request"] = ":loudspeaker:",
        ["Risk"] = ":fire:",
    };
}
