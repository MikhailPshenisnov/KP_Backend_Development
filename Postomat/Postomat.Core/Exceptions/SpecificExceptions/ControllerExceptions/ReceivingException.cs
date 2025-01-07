using Postomat.Core.Exceptions.BaseExceptions;

namespace Postomat.Core.Exceptions.SpecificExceptions.ControllerExceptions;

public class ReceivingException : ControllerException
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