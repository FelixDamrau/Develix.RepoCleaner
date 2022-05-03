using System.CommandLine;
using System.CommandLine.Binding;

namespace Develix.RepoCleaner.Model;

public class ConsoleArgumentsBinder : BinderBase<ConsoleArguments>
{
    private static readonly Option<string?> pathOption = new(
        new[] { "--path", "-p" },
        getDefaultValue: () => null,
        description: "The path to a local repository.");

    private static readonly Option<BranchSourceKind> branchesOption = new(
        new[] { "--branch" },
        getDefaultValue: () => BranchSourceKind.Local,
        description: "Specifies which branches to include.");

    private static readonly Option<bool> pullRequestOption = new(
        new[] { "--pr" },
        getDefaultValue: () => false,
        description: "Include pull requests when loading work items. (Currently no UI counterpart)");

    private static readonly Option<bool> authorOption = new(
        new[] { "--author" },
        getDefaultValue: () => false,
        description: "Include author and date of the last commit.");

    private static readonly Option<bool> configOption = new(
        new[] { "--config" },
        getDefaultValue: () => false,
        description: "Enter configuration mode and allow to set the needed tokens for azure devops.");

    public static RootCommand GetRootCommand()
    {
        return new("RepoCleaner - Stuff with repositories and cleaning...")
        {
            pathOption,
            branchesOption,
            pullRequestOption,
            authorOption,
            configOption
        };
    }

    protected override ConsoleArguments GetBoundValue(BindingContext bindingContext)
    {
        return new ConsoleArguments()
        {
            Author = bindingContext.ParseResult.GetValueForOption(authorOption),
            Branches = bindingContext.ParseResult.GetValueForOption(branchesOption),
            Config = bindingContext.ParseResult.GetValueForOption(configOption),
            Path = bindingContext.ParseResult.GetValueForOption(pathOption),
            Pr = bindingContext.ParseResult.GetValueForOption(pullRequestOption),
        };
    }
}
