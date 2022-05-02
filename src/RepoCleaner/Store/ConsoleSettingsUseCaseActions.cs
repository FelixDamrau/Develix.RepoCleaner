using Develix.RepoCleaner.Model;

namespace Develix.RepoCleaner.Store;

public record SetConsoleSettingsAction(ConsoleArguments ConsoleArguments, AppSettings AppSettings);
