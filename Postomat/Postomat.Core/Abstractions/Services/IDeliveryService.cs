namespace Postomat.Core.Abstractions.Services;

public interface IDeliveryService
{
    Task<Guid> DeliverOrderAsync(Guid orderId, Guid postomatId, CancellationToken ct);
    Task<Guid> DeliverOrderBackAsync(Guid orderId, Guid postomatId, CancellationToken ct);
}