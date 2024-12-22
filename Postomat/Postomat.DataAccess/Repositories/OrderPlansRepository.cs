using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions.Repositories;
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
        if (orderPlan.Status != "Created")
            throw new Exception("You can only create order plans with the status \"Created\"");

        if (orderPlan.StoreUntilDate is not null)
            throw new Exception("The order should not be delivered initially, change the storage time to");

        var existedOrderPlan = await _context.OrderPlans
            .FirstOrDefaultAsync(op => op.OrderId == orderPlan.Order.Id);
        if (existedOrderPlan is not null)
            throw new Exception($"An order plan for the order \"{orderPlan.Order.Id}\" already exists");

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderPlan.Order.Id);
        if (order is null)
            throw new Exception($"Unknown order id: \"{orderPlan.Order.Id}\"");

        var postomat = await _context.Postomats
            .FirstOrDefaultAsync(p => p.Id == orderPlan.Postomat.Id);
        if (postomat is null)
            throw new Exception($"Unknown postomat id: \"{orderPlan.Postomat.Id}\"");

        var creator = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == orderPlan.CreatedBy.Id);
        if (creator is null)
            throw new Exception($"Unknown order plan creator id: \"{orderPlan.CreatedBy.Id}\"");

        if (orderPlan.DeliveredBy is not null || orderPlan.DeliveredBackBy is not null)
            throw new Exception("Created order cannot be delivered initially");

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
        List<User> users;

        try
        {
            postomats = await _postomatsRepository.GetAllPostomats();
            users = await _usersRepository.GetAllUsers();
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred when getting the order plans {e.Message}");
        }

        var orderPlans = new List<OrderPlan>();
        foreach (var orderPlanEntity in orderPlanEntities)
        {
            var order = orders.FirstOrDefault(o => o.Id == orderPlanEntity.OrderId);
            if (order is null)
            {
                await _context.OrderPlans
                    .Where(op => op.Id == orderPlanEntity.Id)
                    .ExecuteDeleteAsync();

                throw new Exception($"Order plan \"{orderPlanEntity.Id}\" has unknown order id " +
                                    $"\"{orderPlanEntity.OrderId}\" and was deleted forcibly");
            }

            var cellWithOrder = await _context.Cells.FirstOrDefaultAsync(c => c.OrderId == order.Id);
            if (orderPlanEntity.Status == "Delivered" && cellWithOrder is null)
            {
                await _context.OrderPlans
                    .Where(op => op.Id == orderPlanEntity.Id)
                    .ExecuteDeleteAsync();

                throw new Exception($"It seems the order \"{order.Id}\" was lost, " +
                                    $"the order plan \"{orderPlanEntity.Id}\" was deleted forcibly");
            }

            var postomat = postomats.FirstOrDefault(p => p.Id == orderPlanEntity.PostomatId);
            if (postomat is null)
            {
                var numUpdated = await _context.Cells
                    .Where(c => c.OrderId == orderPlanEntity.OrderId &&
                                c.PostomatId == orderPlanEntity.PostomatId)
                    .ExecuteDeleteAsync();

                await _context.OrderPlans
                    .Where(op => op.Id == orderPlanEntity.Id)
                    .ExecuteDeleteAsync();

                throw new Exception($"Order plan \"{orderPlanEntity.Id}\" has unknown postomat id " +
                                    $"\"{orderPlanEntity.PostomatId}\", cells containing an order " +
                                    $"from this order plan ({numUpdated} cells) have been cleared " +
                                    $"forcibly and deleted, also the order plan has been deleted");
            }

            var creator = users.FirstOrDefault(u => u.Id == orderPlanEntity.CreatedBy);
            var deliver1 = users.FirstOrDefault(u => u.Id == orderPlanEntity.DeliveredBy);
            var deliver2 = users.FirstOrDefault(u => u.Id == orderPlanEntity.DeliveredBackBy);
            if (creator is null ||
                (orderPlanEntity.DeliveredBy is not null && deliver1 is null) ||
                (orderPlanEntity.DeliveredBackBy is not null && deliver2 is null))
            {
                var numUpdated = await _context.Cells
                    .Where(c => c.OrderId == orderPlanEntity.OrderId)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(c => c.OrderId, c => null));

                await _context.OrderPlans
                    .Where(op => op.Id == orderPlanEntity.Id)
                    .ExecuteDeleteAsync();

                throw new Exception($"Order plan \"{orderPlanEntity.Id}\" has unknown user id for one of users, " +
                                    $"cells containing an order from this order plan ({numUpdated} cells) have been " +
                                    $"cleared forcibly and the order plan has been deleted");
            }

            orderPlans.Add(OrderPlan
                .Create(
                    orderPlanEntity.Id,
                    orderPlanEntity.Status,
                    orderPlanEntity.LastStatusChangeDate,
                    orderPlanEntity.StoreUntilDate,
                    orderPlanEntity.DeliveryCodeHash,
                    Order.Create(order.Id, order.ReceivingCodeHash, order.OrderSize).Order,
                    postomat,
                    creator,
                    deliver1,
                    deliver2).OrderPlan);
        }

        return orderPlans;
    }

    public async Task<Guid> UpdateOrderPlan(Guid orderPlanId, OrderPlan newOrderPlan)
    {
        var oldOrderPlan = (await GetAllOrderPlans()) // It is important to leave the function for internal checks
            .FirstOrDefault(op => op.Id == orderPlanId);

        if (oldOrderPlan is null)
            throw new Exception($"Unknown order plan id: \"{orderPlanId}\"");

        if (oldOrderPlan.Status == "Finished")
            throw new Exception("You cannot edit finished orders");
        if (((oldOrderPlan.Status == "Created" && newOrderPlan.Status != "Delivered") ||
             (oldOrderPlan.Status == "Delivered" && newOrderPlan.Status != "Finished")) &&
            oldOrderPlan.Status != newOrderPlan.Status)
            throw new Exception("You should change the statuses in the order \"Created\" -> " +
                                "\"Delivered\" -> \"Finished\" or not change it");

        var orderEntity = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == newOrderPlan.Order.Id);
        if (orderEntity is null)
            throw new Exception($"New order plan \"{newOrderPlan.Id}\" " +
                                $"has unknown order id \"{newOrderPlan.Order.Id}\"");

        var postomatEntity = await _context.Postomats
            .FirstOrDefaultAsync(p => p.Id == newOrderPlan.Postomat.Id);
        if (postomatEntity is null)
            throw new Exception($"New order plan \"{newOrderPlan.Id}\" " +
                                $"has unknown postomat id \"{newOrderPlan.Postomat.Id}\"");

        var users = await _context.Users.ToListAsync();

        var creator = users
            .FirstOrDefault(u => u.Id == newOrderPlan.CreatedBy.Id);
        if (creator is null)
            throw new Exception($"New order plan \"{newOrderPlan.Id}\" " +
                                $"has unknown creator id \"{newOrderPlan.CreatedBy.Id}\"");

        var deliver1 = users
            .FirstOrDefault(u => newOrderPlan.DeliveredBy != null && u.Id == newOrderPlan.DeliveredBy.Id);
        if (newOrderPlan.DeliveredBy is not null && deliver1 is null)
            throw new Exception($"New order plan \"{newOrderPlan.Id}\" " +
                                $"has unknown deliver1 id \"{newOrderPlan.DeliveredBy.Id}\"");

        var deliver2 = users
            .FirstOrDefault(u => newOrderPlan.DeliveredBackBy != null && u.Id == newOrderPlan.DeliveredBackBy.Id);
        if (newOrderPlan.DeliveredBackBy is not null && deliver2 is null)
            throw new Exception($"New order plan \"{newOrderPlan.Id}\" " +
                                $"has unknown deliver2 id \"{newOrderPlan.DeliveredBackBy.Id}\"");

        var deliveredById = deliver1?.Id;
        var deliveredBackById = deliver2?.Id;

        await _context.OrderPlans
            .Where(op => op.Id == orderPlanId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(op => op.Status, op => newOrderPlan.Status)
                .SetProperty(op => op.LastStatusChangeDate, op => newOrderPlan.LastStatusChangeDate)
                .SetProperty(op => op.StoreUntilDate, op => newOrderPlan.StoreUntilDate)
                .SetProperty(op => op.DeliveryCodeHash, op => newOrderPlan.DeliveryCodeHash)
                .SetProperty(op => op.OrderId, op => newOrderPlan.Order.Id)
                .SetProperty(op => op.PostomatId, op => newOrderPlan.Postomat.Id)
                .SetProperty(op => op.CreatedBy, op => newOrderPlan.CreatedBy.Id)
                .SetProperty(op => op.DeliveredBy, op => deliveredById)
                .SetProperty(op => op.DeliveredBackBy, op => deliveredBackById));
        return orderPlanId;
    }

    public async Task<Guid> DeleteOrderPlan(Guid orderPlanId)
    {
        var orderPlan = (await GetAllOrderPlans()) // It is important to leave the function for internal checks
            .FirstOrDefault(op => op.Id == orderPlanId);

        if (orderPlan is null)
            throw new Exception($"Unknown order plan id: \"{orderPlanId}\"");

        if (orderPlan.Status != "Created" && orderPlan.Status != "Finished")
            throw new Exception("You can only delete created or finished orders");

        await _context.OrderPlans
            .Where(op => op.Id == orderPlanId)
            .ExecuteDeleteAsync();
        await _context.Orders
            .Where(o => o.Id == orderPlan.Order.Id)
            .ExecuteDeleteAsync();

        return orderPlanId;
    }
}