using Develix.CredentialStore.Win32;
using Fluxor;

namespace Develix.RepoCleaner.Store.ConsoleSettingsUseCase;

public class Effects
{
    [EffectMethod]
    public Task HandleConfigureCredentialsAction(ConfigureCredentialsAction action, IDispatcher dispatcher)
    {
        var credential = new Credential("token", action.Token, action.CredentialName);
        var crudResult = CredentialManager.CreateOrUpdate(credential);
        var resultAction = new ConfigureCredentialsResultAction(crudResult);
        dispatcher.Dispatch(resultAction);
        return Task.CompletedTask;
    }
}
