using Postomat.Core.Models;

namespace Postomat.Core.Abstractions.Repositories;

public interface IOrderPlansRepository
{
    Task<Guid> CreateOrderPlan(OrderPlan orderPlan);
    Task<List<OrderPlan>> GetAllOrderPlans();
    Task<Guid> UpdateOrderPlan(Guid orderPlanId, OrderPlan newOrderPlan);
    Task<Guid> DeleteOrderPlan(Guid orderPlanId);
}