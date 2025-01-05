using Postomat.Core.Exceptions.BaseExceptions;

namespace Postomat.Core.Exceptions.SpecificExceptions;

public class ReceivingException : ServiceException
{
    public ReceivingException()
    {
    }

    public ReceivingException(string message)
        : base(message)
    {
    }

    public ReceivingException(string message, Exception inner)
        : base(message, inner)
    {
    }
}