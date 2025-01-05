namespace Postomat.Core.Exceptions.BaseExceptions;

public class RepositoryException : ExpectedException
{
    public RepositoryException()
    {
    }

    public RepositoryException(string message)
        : base(message)
    {
    }

    public RepositoryException(string message, Exception inner) : base(message, inner)
    {
    }
}