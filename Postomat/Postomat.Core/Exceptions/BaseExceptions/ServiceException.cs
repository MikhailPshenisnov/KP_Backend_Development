namespace Postomat.Core.Exceptions.BaseExceptions;

public class ServiceException : ExpectedException
{
    public ServiceException()
    {
    }

    public ServiceException(string message)
        : base(message)
    {
    }

    public ServiceException(string message, Exception inner) : base(message, inner)
    {
    }
}