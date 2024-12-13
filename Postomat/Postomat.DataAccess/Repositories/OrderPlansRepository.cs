using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class OrderPlansRepository : IOrderPlansRepository
{
    private readonly PostomatDbContext _context;
    private readonly IPostomatsRepository _postomatsRepository;
    private readonly IUsersRepository _usersRepository;

    public OrderPlansRepository(PostomatDbContext context, IPostomatsRepository postomatsRepository, 
        IUsersRepository usersRepository)
    {
        _context = context;
        _postomatsRepository = postomatsRepository;
        _usersRepository = usersRepository;
    }

    public async Task<Guid> CreateOrderPlan(OrderPlan orderPlan)
    {
        if (orderPlan.Status != "Создан")
            throw new Exception("You can only create order plans with the status \"Создан\"");
        
        if (orderPlan.StoreUntilDate is not null)
            throw new Exception("The order should not be delivered initially, change the storage time to");
        
        /* TODO */
        
        // Существование заказа
        
        // Существование постамата
        
        // Существование пользователя
        
        // Проверка на отсутствие других пользователей
        
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

        return orderPlanEntity.Id;
    }

    public async Task<List<OrderPlan>> GetAllOrderPlans()
    {
        var orderPlanEntities = await _context.OrderPlans
            .AsNoTracking()
            .ToListAsync();
        
        var orders = await _context.Orders.ToListAsync();
        
        List<Core.Models.Postomat> postomats;
        try
        {
            postomats = await _postomatsRepository.GetAllPostomats();
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred when getting the order plans {e.Message}");
        }
        
        List<User> users;
        try
        {
            users = await _usersRepository.GetAllUsers();
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred when getting the order plans {e.Message}");
        }

        var orderPlans = new List<OrderPlan>();
        foreach (var orderPlanEntity in orderPlanEntities)
        {
            /* TODO */
            /*var cells = (await _cellsRepository.GetAllCells())
                .Where(c => c.OrderPlanId == orderPlanEntity.Id)
                .ToList();

            orderPlans.Add(Core.Models.OrderPlan
                .Create(
                    orderPlanEntity.Id,
                    orderPlanEntity.Name,
                    orderPlanEntity.Address,
                    cells)
                .OrderPlan);*/
        }

        return orderPlans;
    }

    /* TODO */
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

    /* TODO */
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