using Develix.Essentials.Core;
using Develix.RepoCleaner.Model;

namespace Develix.RepoCleaner.Store;

public record SetConsoleSettingsAction(ConsoleArguments ConsoleArguments, AppSettings AppSettings);

public record ConfigureCredentialsAction(string CredentialName, string Token);

public record ConfigureCredentialsResultAction(Result Result);
