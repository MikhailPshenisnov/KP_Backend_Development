using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Application.Services;

public class PostomatsService : IPostomatsService
{
    private readonly IPostomatsRepository _postomatsRepository;
    private readonly ICellsRepository _cellsRepository;
    private readonly IOrderPlansRepository _orderPlansRepository;

    public PostomatsService(IPostomatsRepository postomatsRepository, ICellsRepository cellsRepository,
        IOrderPlansRepository orderPlansRepository)
    {
        _postomatsRepository = postomatsRepository;
        _cellsRepository = cellsRepository;
        _orderPlansRepository = orderPlansRepository;
    }

    public async Task<Guid> CreatePostomatAsync(Core.Models.Postomat postomat, CancellationToken ct)
    {
        try
        {
            var createdPostomatId = await _postomatsRepository.CreatePostomat(postomat);

            return createdPostomatId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to create postomat \"{postomat.Id}\": \"{e.Message}\"");
        }
    }

    public async Task<Core.Models.Postomat> GetPostomatAsync(Guid postomatId, CancellationToken ct)
    {
        try
        {
            var allPostomats = await _postomatsRepository.GetAllPostomats();

            var postomat = allPostomats.FirstOrDefault(p => p.Id == postomatId);
            if (postomat == null)
                throw new Exception($"Unknown postomat id: \"{postomatId}\"");

            return postomat;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to get postomat \"{postomatId}\": \"{e.Message}\"");
        }
    }

    public async Task<List<Core.Models.Postomat>> GetFilteredPostomatsAsync(PostomatFilter? postomatFilter,
        CancellationToken ct)
    {
        try
        {
            var postomats = await _postomatsRepository.GetAllPostomats();

            if (postomatFilter == null)
                return postomats;

            if (postomatFilter.PartOfName is not null)
            {
                postomats = postomats
                    .Where(p => p.Name.ToLower().Contains(postomatFilter.PartOfName.ToLower()))
                    .ToList();
            }

            if (postomatFilter.PartOfAddress is not null)
            {
                postomats = postomats
                    .Where(p => p.Address.ToLower().Contains(postomatFilter.PartOfAddress.ToLower()))
                    .ToList();
            }

            return postomats.OrderBy(x => x.Name).ToList();
        }

        catch (Exception e)
        {
            throw new Exception($"Unable to get filtered postomats: \"{e.Message}\"");
        }
    }

    public async Task<Guid> UpdatePostomatAsync(Guid postomatId, Core.Models.Postomat newPostomat, CancellationToken ct)
    {
        try
        {
            var updatedPostomatId = await _postomatsRepository.UpdatePostomat(postomatId, newPostomat);

            return updatedPostomatId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to update postomat \"{postomatId}\": \"{e.Message}\"");
        }
    }

    public async Task<Guid> DeletePostomatAsync(Guid postomatId, CancellationToken ct)
    {
        try
        {
            var existedPostomatId = await _postomatsRepository.DeletePostomat(postomatId);

            return existedPostomatId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to delete postomat \"{postomatId}\": \"{e.Message}\"");
        }
    }

    public async Task<Guid> AddCellToPostomatAsync(Cell cell, CancellationToken ct)
    {
        try
        {
            var createdCellId = await _cellsRepository.CreateCell(cell);

            return createdCellId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to add cell \"{cell.Id}\" to postomat \"{cell.PostomatId}\": \"{e.Message}\"");
        }
    }

    public async Task<(Guid CellId, Guid OrderPlanId)> FillCellInPostomatAsync(User user, Guid postomatId, Order order,
        CancellationToken ct)
    {
        try
        {
            var postomat = await GetPostomatAsync(postomatId, ct);
            var suitableCell = postomat.Cells.FirstOrDefault(c => c.CellSize == order.OrderSize) ??
                               (postomat.Cells.FirstOrDefault(c => c.CellSize == order.OrderSize + 1) ??
                                postomat.Cells.FirstOrDefault(c => c.CellSize == order.OrderSize + 2));
            if (suitableCell is null)
                throw new Exception($"No suitable cell in the postomat \"{postomatId}\"");

            var orderPlan = (await _orderPlansRepository.GetAllOrderPlans())
                .FirstOrDefault(op => op.Order.Id == order.Id);
            if (orderPlan is null)
                throw new Exception($"Order \"{order.Id}\" has no order plan");

            var updatedOrderPlan = OrderPlan.Create(
                orderPlan.Id,
                "Delivered",
                DateTime.Now.ToUniversalTime(),
                DateTime.Now.AddDays(3).ToUniversalTime(),
                orderPlan.DeliveryCodeHash,
                orderPlan.Order,
                orderPlan.Postomat,
                orderPlan.CreatedBy,
                user,
                orderPlan.DeliveredBackBy);
            if (updatedOrderPlan.Error is not null)
                throw new Exception($"Unable to update order plan for order \"{order.Id}\": {updatedOrderPlan.Error}");

            var updatedCell = Cell.Create(
                suitableCell.Id,
                suitableCell.CellSize,
                suitableCell.PostomatId,
                order);
            if (updatedCell.Error is not null)
                throw new Exception($"Unable to fill cell with order \"{order.Id}\": {updatedCell.Error}");

            var updatedOrderPlanId = await _orderPlansRepository
                .UpdateOrderPlan(orderPlan.Id, updatedOrderPlan.OrderPlan);
            var updatedCellId = await _cellsRepository
                .UpdateCell(suitableCell.Id, updatedCell.Cell);

            return (updatedCellId, updatedOrderPlanId);
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to fill cell in postomat \"{postomatId}\": \"{e.Message}\"");
        }
    }

    public async Task<(Guid CellId, Guid OrderPlanId)> ClearCellInPostomatAsync(User user, Guid postomatId, Order order,
        CancellationToken ct)
    {
        try
        {
            var postomat = await GetPostomatAsync(postomatId, ct);
            var cellWithOrder = postomat.Cells.FirstOrDefault(c => c.Order?.Id == order.Id);
            if (cellWithOrder is null)
                throw new Exception($"No cell with order \"{order.Id}\" in the postomat \"{postomatId}\"");

            var orderPlan = (await _orderPlansRepository.GetAllOrderPlans())
                .FirstOrDefault(op => op.Order.Id == order.Id);
            if (orderPlan is null)
                throw new Exception($"Order \"{order.Id}\" has no order plan");

            var updatedOrderPlan = OrderPlan.Create(
                orderPlan.Id,
                "Finished",
                DateTime.Now.ToUniversalTime(),
                null,
                orderPlan.DeliveryCodeHash,
                orderPlan.Order,
                orderPlan.Postomat,
                orderPlan.CreatedBy,
                orderPlan.DeliveredBy,
                user);
            if (updatedOrderPlan.Error is not null)
                throw new Exception($"Unable to update order plan for order \"{order.Id}\": {updatedOrderPlan.Error}");

            var updatedCell = Cell.Create(
                cellWithOrder.Id,
                cellWithOrder.CellSize,
                cellWithOrder.PostomatId,
                null);
            if (updatedCell.Error is not null)
                throw new Exception($"Unable to clear cell with order \"{order.Id}\": {updatedCell.Error}");

            var updatedOrderPlanId = await _orderPlansRepository
                .UpdateOrderPlan(orderPlan.Id, updatedOrderPlan.OrderPlan);
            var updatedCellId = await _cellsRepository
                .UpdateCell(cellWithOrder.Id, updatedCell.Cell);

            return (updatedCellId, updatedOrderPlanId);
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to clear cell in postomat \"{postomatId}\": \"{e.Message}\"");
        }
    }

    public async Task<Guid> DeleteCellFromPostomatAsync(Guid cellId, CancellationToken ct)
    {
        try
        {
            var existedCellId = await _cellsRepository.DeleteCell(cellId);

            return existedCellId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to delete cell \"{cellId}\": \"{e.Message}\"");
        }
    }
}