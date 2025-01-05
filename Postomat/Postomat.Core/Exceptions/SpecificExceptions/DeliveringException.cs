using Postomat.Core.Exceptions.BaseExceptions;

namespace Postomat.Core.Exceptions.SpecificExceptions;

public class DeliveringException : ServiceException
{
    public DeliveringException()
    {
    }

    public DeliveringException(string message)
        : base(message)
    {
    }

    public DeliveringException(string message, Exception inner)
        : base(message, inner)
    {
    }
}