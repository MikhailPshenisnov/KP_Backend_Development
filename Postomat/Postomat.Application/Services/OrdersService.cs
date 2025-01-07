using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Application.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;

    public OrdersService(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<Guid> CreateOrderAsync(Order order, CancellationToken ct)
    {
        try
        {
            var createdOrderId = await _ordersRepository.CreateOrder(order);

            return createdOrderId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to create order \"{order.Id}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Order> GetOrderAsync(Guid orderId, CancellationToken ct)
    {
        try
        {
            var allOrders = await _ordersRepository.GetAllOrders();

            var order = allOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                throw new UnknownIdentifierException($"Unknown order id: \"{orderId}\".");

            return order;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to get order \"{orderId}\". " +
                                $"--> {e.Message}");
        }
    }

    public async Task<List<Order>> GetFilteredOrdersAsync(OrderFilter? orderFilter, CancellationToken ct)
    {
        try
        {
            var orders = await _ordersRepository.GetAllOrders();

            if (orderFilter == null)
                return orders;

            if (orderFilter.OrderSizeFrom is not null)
            {
                orders = orders
                    .Where(o => o.OrderSize >= orderFilter.OrderSizeFrom)
                    .ToList();
            }

            if (orderFilter.OrderSizeTo is not null)
            {
                orders = orders
                    .Where(o => o.OrderSize <= orderFilter.OrderSizeTo)
                    .ToList();
            }

            return orders.OrderBy(x => x.Id).ToList();
        }

        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to get filtered orders. " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> UpdateOrderAsync(Guid orderId, Order newOrder, CancellationToken ct)
    {
        try
        {
            var updatedOrderId = await _ordersRepository.UpdateOrder(orderId, newOrder);

            return updatedOrderId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to update order \"{orderId}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> DeleteOrderAsync(Guid orderId, CancellationToken ct)
    {
        try
        {
            var existedOrderId = await _ordersRepository.DeleteOrder(orderId);

            return existedOrderId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to delete order \"{orderId}\". " +
                                       $"--> {e.Message}");
        }
    }
}