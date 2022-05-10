namespace Develix.RepoCleaner.Utils;
public static class AsyncHelper
{
    public static async Task WaitUntilAsync(Func<bool> condition, int pollDelay, CancellationToken cancellationToken)
    {
        while (!condition())
        {
            await Task.Delay(pollDelay, cancellationToken).ConfigureAwait(true);
        }
    }

    public static async Task WaitUntilAsync(Func<bool> condition, int pollDelay, int timeout, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        await WaitUntilAsync(condition, pollDelay, cts.Token).ConfigureAwait(true);

    }
}
