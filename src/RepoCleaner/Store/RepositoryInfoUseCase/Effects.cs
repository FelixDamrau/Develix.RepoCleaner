using Develix.AzureDevOps.Connector.Service;
using Develix.RepoCleaner.Store.ConsoleSettingsUseCase;
using Fluxor;

namespace Develix.RepoCleaner.Store.RepositoryInfoUseCase;
public class Effects
{
    private readonly IState<ConsoleSettingsState> consoleSettingsState;
    private readonly IWorkItemService workItemService;

    public Effects(IState<ConsoleSettingsState> consoleSettingsState, IWorkItemService workItemService)
    {
        this.consoleSettingsState = consoleSettingsState;
        this.workItemService = workItemService;
    }

    [EffectMethod(typeof(InitRepositoryAction))]
    public Task HandleInitRepositoryAction(IDispatcher dispatcher)
    {
        var path = consoleSettingsState.Value.Path ?? Directory.GetCurrentDirectory();
        var repository = Git.Reader.GetLocalRepo(path);
        var setRepositoryAction = new SetRepositoryAction(repository);
        dispatcher.Dispatch(setRepositoryAction);
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleSetRepositoryAction(SetRepositoryAction action, IDispatcher dispatcher)
    {
        var ids = action.Repository.Branches.Select(b => b.RelatedWorkItemId).OfType<int>();
        var workItemsResult = await workItemService.GetWorkItems(ids, false);

        if (workItemsResult.Valid)
        {
            var setWorkItemsAction = new SetWorkItemsAction(workItemsResult.Value.ToList());
            dispatcher.Dispatch(setWorkItemsAction);
        }
    }
}
