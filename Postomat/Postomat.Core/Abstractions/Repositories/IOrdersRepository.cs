using Postomat.Core.Models;

namespace Postomat.Core.Abstractions.Repositories;

public interface IOrdersRepository
{
    Task<Guid> CreateOrder(Order order);
    Task<List<Order>> GetAllOrders();
    Task<Guid> UpdateOrder(Guid orderId, Order newOrder);
    Task<Guid> DeleteOrder(Guid orderId);
}