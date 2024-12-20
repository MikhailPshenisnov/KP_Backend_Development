using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly PostomatDbContext _context;

    public OrdersRepository(PostomatDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateOrder(Order order)
    {
        var orderEntity = new Database.Entities.Order
        {
            Id = order.Id,
            ReceivingCodeHash = order.ReceivingCodeHash,
            OrderSize = order.OrderSize
        };

        await _context.Orders.AddAsync(orderEntity);
        await _context.SaveChangesAsync();

        return orderEntity.Id;
    }

    public async Task<List<Order>> GetAllOrders()
    {
        var orderEntities = await _context.Orders
            .AsNoTracking()
            .ToListAsync();

        var orders = orderEntities
            .Select(orderEntity => Order
                .Create(
                    orderEntity.Id,
                    orderEntity.ReceivingCodeHash,
                    orderEntity.OrderSize)
                .Order)
            .ToList();

        return orders;
    }

    public async Task<Guid> UpdateOrder(Guid orderId, Order newOrder)
    {
        var oldOrderEntity = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (oldOrderEntity is null)
            throw new Exception($"Unknown order id: \"{orderId}\"");

        var cellWithOrderEntity = await _context.Cells
            .FirstOrDefaultAsync(c => c.OrderId == oldOrderEntity.Id);
        if (cellWithOrderEntity is not null && newOrder.OrderSize > cellWithOrderEntity.CellSize)
            throw new Exception($"It is impossible to change the order size for order \"{orderId}\", " +
                                $"due to the discrepancy between the new size and the size of the " +
                                $"cell \"{cellWithOrderEntity.Id}\" in which the order is already stored");

        await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(o => o.ReceivingCodeHash, o => newOrder.ReceivingCodeHash)
                .SetProperty(o => o.OrderSize, o => newOrder.OrderSize));

        return orderId;
    }

    public async Task<Guid> DeleteOrder(Guid orderId)
    {
        var cellWithOrderEntity = await _context.Cells
            .FirstOrDefaultAsync(c => c.OrderId == orderId);
        if (cellWithOrderEntity is not null)
            throw new Exception($"Deleting an order \"{orderId}\" is destructive, " +
                                $"it is contained in a cell \"{cellWithOrderEntity.Id}\" " +
                                $"at the postomat \"{cellWithOrderEntity.PostomatId}\"");

        var orderPlanWithOrderEntity = await _context.OrderPlans
            .FirstOrDefaultAsync(op => op.Order.Id == orderId);
        if (orderPlanWithOrderEntity is not null)
            throw new Exception($"Deleting an order \"{orderId}\" is destructive, " +
                                $"it is contained in an order plan \"{orderPlanWithOrderEntity.Id}\"");

        var numUpdated = await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteDeleteAsync();

        if (numUpdated == 0)
            throw new Exception($"Unknown order id: \"{orderId}\"");

        return orderId;
    }
}