using Postomat.Core.Exceptions.BaseExceptions;

namespace Postomat.Core.Exceptions.SpecificExceptions.RepositoryExceptions;

public class UnknownIdentifierException : RepositoryException
{
    public UnknownIdentifierException()
    {
    }

    public UnknownIdentifierException(string message)
        : base(message)
    {
    }

    public UnknownIdentifierException(string message, Exception inner)
        : base(message, inner)
    {
    }
}