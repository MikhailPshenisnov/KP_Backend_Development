using Postomat.Core.Exceptions.BaseExceptions;

namespace Postomat.Core.Exceptions.SpecificExceptions.ControllerExceptions;

public class DeliveringException : ControllerException
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