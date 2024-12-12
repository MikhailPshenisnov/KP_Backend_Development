using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class OrderPlansRepository : IOrderPlansRepository
{
    private readonly PostomatDbContext _context;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IPostomatsRepository _postomatsRepository;
    private readonly IUsersRepository _usersRepository;

    public OrderPlansRepository(PostomatDbContext context, IOrdersRepository ordersRepository, 
        IPostomatsRepository postomatsRepository, IUsersRepository usersRepository)
    {
        _context = context;
        _ordersRepository = ordersRepository;
        _postomatsRepository = postomatsRepository;
        _usersRepository = usersRepository;
    }

    public async Task<Guid> CreateOrderPlan(OrderPlan orderPlan)
    {
        var orderPlanEntity = new Database.Entities.OrderPlan
        {
            Id = orderPlan.Id,
            Status = orderPlan.Status,
            LastStatusChangeDate = orderPlan.LastStatusChangeDate,
            StoreUntilDate = orderPlan.StoreUntilDate,
            DeliveryCodeHash = orderPlan.DeliveryCodeHash,
            OrderId = orderPlan.Order.Id,
            PostomatId = orderPlan.Postomat.Id,
            CreatedBy = orderPlan.CreatedBy.Id,
            DeliveredBy = orderPlan.DeliveredBy?.Id,
            DeliveredBackBy = orderPlan.DeliveredBackBy?.Id
        };

        await _context.OrderPlans.AddAsync(orderPlanEntity);
        await _context.SaveChangesAsync();

        var addedCells = new List<Guid>();
        foreach (var cell in orderPlan.Cells)
        {
            addedCells.Add(await _cellsRepository.CreateCell(cell));
        }

        return orderPlanEntity.Id;
    }

    public async Task<List<OrderPlan>> GetAllOrderPlans()
    {
        var orderPlanEntities = await _context.OrderPlans
            .AsNoTracking()
            .ToListAsync();

        var orderPlans = new List<OrderPlan>();
        foreach (var orderPlanEntity in orderPlanEntities)
        {
            var cells = (await _cellsRepository.GetAllCells())
                .Where(c => c.OrderPlanId == orderPlanEntity.Id)
                .ToList();

            orderPlans.Add(Core.Models.OrderPlan
                .Create(
                    orderPlanEntity.Id,
                    orderPlanEntity.Name,
                    orderPlanEntity.Address,
                    cells)
                .OrderPlan);
        }

        return orderPlans;
    }

    public async Task<Guid> UpdateOrderPlan(Guid orderPlanId, OrderPlan newOrderPlan)
    {
        var oldOrderPlan = (await GetAllOrderPlans())
            .FirstOrDefault(op => op.Id == orderPlanId);

        if (oldOrderPlan is null)
            throw new Exception("Unknown orderPlan id");

        foreach (var cell in oldOrderPlan.Cells)
        {
            await _cellsRepository.DeleteCell(cell.Id);
        }

        await _context.OrderPlans
            .Where(op => op.Id == orderPlanId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(op => op.Name, op => newOrderPlan.Name)
                .SetProperty(op => op.Address, op => newOrderPlan.Address));

        foreach (var cell in newOrderPlan.Cells)
        {
            await _cellsRepository.CreateCell(cell);
        }

        return orderPlanId;
    }

    public async Task<Guid> DeleteOrderPlan(Guid orderPlanId)
    {
        var numUpdated = await _context.OrderPlans
            .Where(op => op.Id == orderPlanId)
            .ExecuteDeleteAsync();

        if (numUpdated == 0)
        {
            throw new Exception("Unknown orderPlan id");
        }

        return orderPlanId;
    }
}