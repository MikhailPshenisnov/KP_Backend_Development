using Postomat.Core.Models.Other;

namespace Postomat.Core.Models;

public class Order
{
    public const int MaxReceivingCodeHashLength = 128;

    private Order(Guid id, string receivingCodeHash, SizeEnumerator orderSize)
    {
        Id = id;
        ReceivingCodeHash = receivingCodeHash;
        OrderSize = orderSize;
    }

    public Guid Id { get; }
    public string ReceivingCodeHash { get; }
    public SizeEnumerator OrderSize { get; }

    private static string BasicChecks(string receivingCodeHash)
    {
        var error = string.Empty;

        if (string.IsNullOrEmpty(receivingCodeHash) || receivingCodeHash.Length > MaxReceivingCodeHashLength)
        {
            error = $"Receiving code hash can't be longer than {MaxReceivingCodeHashLength} characters or empty.";
        }

        return error;
    }

    public static (Order Order, string Error) Create(Guid id, string receivingCodeHash, SizeEnumerator orderSize)
    {
        var error = BasicChecks(receivingCodeHash);

        var order = new Order(id, receivingCodeHash, orderSize);

        return (order, error);
    }

    public static string ReceivingCodeCheck(string receivingCode)
    {
        var receivingCodeCheckError = string.Empty;

        if (receivingCode.Length < 8)
        {
            receivingCodeCheckError = "The receiving code must be longer than 8 characters.";
        }
        else if (!receivingCode.All(char.IsLetterOrDigit))
        {
            receivingCodeCheckError = "The receiving code must consist only of letters and numbers.";
        }

        return receivingCodeCheckError;
    }
}