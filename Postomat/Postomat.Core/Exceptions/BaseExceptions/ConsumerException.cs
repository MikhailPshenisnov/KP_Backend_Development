namespace Postomat.Core.Exceptions.BaseExceptions;

public class ConsumerException : ExpectedException
{
    public ConsumerException()
    {
    }

    public ConsumerException(string message)
        : base(message)
    {
    }

    public ConsumerException(string message, Exception inner) : base(message, inner)
    {
    }
}