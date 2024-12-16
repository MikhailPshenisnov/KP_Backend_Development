namespace Postomat.Core.Abstractions.Services;

public interface ICustomerService
{
    Task<Guid> ReceiveOrderAsync(string receivingCode, Guid postomatId, CancellationToken ct);
}