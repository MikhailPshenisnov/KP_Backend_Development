using Postomat.Core.Exceptions.BaseExceptions;

namespace Postomat.Core.Exceptions.SpecificExceptions;

public class DestructiveActionException : RepositoryException
{
    public DestructiveActionException()
    {
    }

    public DestructiveActionException(string message)
        : base(message)
    {
    }

    public DestructiveActionException(string message, Exception inner)
        : base(message, inner)
    {
    }
}