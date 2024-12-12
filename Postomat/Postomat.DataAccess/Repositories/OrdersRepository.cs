using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly PostomatDbContext _context;
    private readonly ICellsRepository _cellsRepository;
    private readonly IOrderPlansRepository _orderPlansRepository;

    public OrdersRepository(
        PostomatDbContext context,
        ICellsRepository cellsRepository,
        IOrderPlansRepository orderPlansRepository)
    {
        _context = context;
        _cellsRepository = cellsRepository;
        _orderPlansRepository = orderPlansRepository;
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
        var oldOrder = (await GetAllOrders())
            .FirstOrDefault(o => o.Id == orderId);
        if (oldOrder is null)
            throw new Exception($"Unknown order id: \"{orderId}\"");

        var cellWithOrder = (await _cellsRepository.GetAllCells())
            .FirstOrDefault(cell => cell.Order?.Id == orderId);
        if (cellWithOrder is not null && newOrder.OrderSize > cellWithOrder.CellSize)
            throw new Exception($"It is impossible to change the order size for order \"{orderId}\", " +
                                $"due to the discrepancy between the new size and the size of the " +
                                $"cell \"{cellWithOrder.Id}\" in which the order is already stored");

        await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(o => o.ReceivingCodeHash, o => newOrder.ReceivingCodeHash)
                .SetProperty(o => o.OrderSize, o => newOrder.OrderSize));

        return orderId;
    }

    public async Task<Guid> DeleteOrder(Guid orderId)
    {
        var cellWithOrder = (await _cellsRepository.GetAllCells())
            .FirstOrDefault(cell => cell.Order?.Id == orderId);
        if (cellWithOrder is not null)
            throw new Exception($"Deleting an order \"{orderId}\" is destructive, " +
                                $"it is contained in a cell \"{cellWithOrder.Id}\" " +
                                $"at the postomat \"{cellWithOrder.PostomatId}\"");

        var orderPlanWithOrder = (await _orderPlansRepository.GetAllOrderPlans())
            .FirstOrDefault(orderPlan => orderPlan.Order.Id == orderId);
        if (orderPlanWithOrder is not null)
            throw new Exception($"Deleting an order \"{orderId}\" is destructive, " +
                                $"it is contained in an order plan \"{orderPlanWithOrder.Id}\"");

        var numUpdated = await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteDeleteAsync();

        if (numUpdated == 0)
            throw new Exception($"Unknown order id: \"{orderId}\"");

        return orderId;
    }
}