using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Develix.RepoCleaner.Model;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Develix.RepoCleaner.ConsoleComponents.Cli;

internal class ConfigCommand : AsyncCommand
{
    private const string EditorEnvironmentVariableName = "EDITOR";
    private const string ConfigFile = "repo-cleaner.yml";

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        StartEditorProcess();
        var configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoCleaner");
        var configFilePath = Path.Combine(configDirectory, ConfigFile);
        await CreateConfigIfNotExistAsync(configFilePath).ConfigureAwait(false);

        return 0;

    }

    private async Task CreateConfigIfNotExistAsync(string configFilePath)
    {
        var configDirectory = Path.GetDirectoryName(configFilePath) ?? throw new UnreachableException($"The config file path could not be parsed. [{configFilePath}]");
        if (!Directory.Exists(configDirectory))
            Directory.CreateDirectory(configDirectory);

        if (!File.Exists(configFilePath))
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
            var appSettings = serializer.Serialize(new AppSettings());
            await File.WriteAllTextAsync(configFilePath, appSettings).ConfigureAwait(false);
        }
    }

    private async Task<AppSettings> GetAppSettingsAsync(string configFilePath)
    {
        var appSettingsYml = await File.ReadAllTextAsync(configFilePath).ConfigureAwait(false)
            ?? throw new UnreachableException($"The config file path could not be read. [{configFilePath}]");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<AppSettings>(appSettingsYml);
    }

    private Process StartEditorProcess()
    {
        var path = Environment.GetEnvironmentVariable("PATH")!.Split(';').FirstOrDefault(p => File.Exists(Path.Combine(p, "code")));
        var editorVariable = Environment.GetEnvironmentVariable(EditorEnvironmentVariableName);
        if (string.IsNullOrEmpty(editorVariable))
        {
            var processStartInfo = GetGitProcessStartInfo("code.cmd", "" + "C:\\temp\\x.txt", path);
            
            return Process.Start(processStartInfo) ?? throw new UnreachableException("");
        }
        return Process.Start(GetGitProcessStartInfo("TODO", "", "")) ?? throw new UnreachableException("");
    }

    private static ProcessStartInfo GetGitProcessStartInfo(string fileName, string arguments, string path)
    {
        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = false,
            WorkingDirectory = path,
        };
    }
}
