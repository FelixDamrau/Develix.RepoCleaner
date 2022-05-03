namespace Develix.RepoCleaner.Model;
public class AppSettings
{
    internal const string SettingsSection = "Settings";

    public Uri AzureDevopsOrgUri { get; set; } = null!;

    /// <summary>
    /// A collection of branches to exclude. Use Regex patterns.
    /// </summary>
    /// <example>Use `^release` to exclude all branches that start with release or `^main$` to exclude the main branch </example>
    public List<string> ExcludedBranches { get; set; } = new();
}
