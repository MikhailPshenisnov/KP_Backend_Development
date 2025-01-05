namespace Postomat.Core.Exceptions.BaseExceptions;

public class ControllerException : ExpectedException
{
    public ControllerException()
    {
    }

    public ControllerException(string message)
        : base(message)
    {
    }

    public ControllerException(string message, Exception inner) : base(message, inner)
    {
    }
}