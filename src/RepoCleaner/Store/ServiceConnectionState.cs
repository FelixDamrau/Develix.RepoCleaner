namespace Develix.RepoCleaner.Store;

public enum ServiceConnectionState
{
    Invalid = 0,
    Disconnected,
    Connecting,
    Connected,
    FailedToConnect,
}
