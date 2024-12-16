using Postomat.Core.Models;

namespace Postomat.Core.Abstractions.Services;

public interface IDeliveryService
{
    Task<Guid> DeliverOrderAsync(User user, string deliveryCode, Guid postomatId, CancellationToken ct);
    Task<Guid> DeliverOrderBackAsync(User user, string deliveryCode, Guid postomatId, CancellationToken ct);
}