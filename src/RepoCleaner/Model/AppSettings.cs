using Microsoft.Extensions.Configuration;

namespace Develix.RepoCleaner.Model;

public class AppSettings
{
    internal const string SettingsSection = "Settings";

    public Uri AzureDevopsOrgUri { get; set; } = null!;

    /// <summary>
    /// A collection of branches to exclude. Use Regex patterns.
    /// </summary>
    /// <example>Use `^release` to exclude all branches that start with release or `^main$` to exclude the main branch </example>
    public List<string> ExcludedBranches { get; set; } = [];

    public GitHandlerKind GitHandler { get; set; }
    public Dictionary<string, string> ShortProjectNames { get; set; } = [];
    public Dictionary<string, string> WorkItemTypeIcons { get; set; } = [];

    public static AppSettings Create()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            ;
        if (Environment.GetEnvironmentVariable("DEV_ENVIRONMENT") is string env)
            configurationBuilder.AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: false);

        var configuration = configurationBuilder.Build();
        var settings = new AppSettings();
        var settingsSection = configuration.GetSection("Settings");
        settingsSection.Bind(settings);

        return settings;
    }
}
