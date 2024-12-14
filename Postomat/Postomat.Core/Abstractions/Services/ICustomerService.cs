namespace Postomat.Core.Abstractions.Services;

public interface ICustomerService
{
    Task<Guid> ReceiveOrderAsync(Guid orderId, Guid postomatId, CancellationToken ct);
}