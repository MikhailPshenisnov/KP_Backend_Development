using Postomat.Core.Models;

namespace Postomat.Core.Abstractions.Services;

public interface IDeliveryService
{
    Task<List<Guid>> DeliverOrderAsync(User user, string deliveryCode, Guid postomatId, CancellationToken ct);
    Task<List<Guid>> DeliverOrderBackAsync(User user, string deliveryCode, Guid postomatId, CancellationToken ct);
}