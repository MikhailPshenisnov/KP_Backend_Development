using Postomat.Core.Models;
using Postomat.Core.Models.Filters;
using Postomat.Core.Models.Filtres;

namespace Postomat.Core.Abstractions.Services;

public interface IOrdersService
{
    Task<Guid> CreateOrderAsync(Order order, CancellationToken ct);
    Task<Order> GetOrderAsync(Guid orderId, CancellationToken ct);
    Task<List<Order>> GetFilteredOrdersAsync(OrderFilter? orderFilter, CancellationToken ct);
    Task<Guid> UpdateOrderAsync(Guid orderId, Order newOrder, CancellationToken ct);
    Task<Guid> DeleteOrderAsync(Guid orderId, CancellationToken ct);
}