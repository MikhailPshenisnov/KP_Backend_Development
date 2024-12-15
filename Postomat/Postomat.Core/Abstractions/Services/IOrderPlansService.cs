using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Core.Abstractions.Services;

public interface IOrderPlansService
{
    Task<Guid> CreateOrderPlanAsync(OrderPlan orderPlan, CancellationToken ct);
    Task<OrderPlan> GetOrderPlanAsync(Guid orderPlanId, CancellationToken ct);
    Task<List<OrderPlan>> GetFilteredOrderPlansAsync(OrderPlanFilter? orderPlanFilter, CancellationToken ct);
    Task<Guid> UpdateOrderPlanAsync(Guid orderPlanId, OrderPlan newOrderPlan, CancellationToken ct);
    Task<Guid> DeleteOrderPlanAsync(Guid orderPlanId, CancellationToken ct);
}