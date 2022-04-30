using System.Runtime.Serialization;

namespace Develix.RepoCleaner.Git;

public class GitHandlerException : Exception
{
    public GitHandlerException()
    {
    }

    public GitHandlerException(string? message) : base(message)
    {
    }

    public GitHandlerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GitHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
