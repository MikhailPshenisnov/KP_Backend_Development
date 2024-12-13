using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class CellsRepository : ICellsRepository
{
    private readonly PostomatDbContext _context;

    public CellsRepository(PostomatDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateCell(Cell cell)
    {
        if (cell.Order is not null)
            throw new Exception("You cannot create a cell with an order inside");

        var postomat = (await _context.Postomats.ToListAsync())
            .FirstOrDefault(p => p.Id == cell.PostomatId);
        if (postomat is null)
            throw new Exception($"Cell \"{cell.Id}\" has unknown postomat id \"{cell.PostomatId}\"");

        var cellEntity = new Database.Entities.Cell
        {
            Id = cell.Id,
            CellSize = cell.CellSize,
            PostomatId = cell.PostomatId,
            OrderId = cell.Order?.Id
        };

        await _context.Cells.AddAsync(cellEntity);
        await _context.SaveChangesAsync();

        return cellEntity.Id;
    }

    public async Task<List<Cell>> GetAllCells()
    {
        var cellEntities = await _context.Cells
            .AsNoTracking()
            .ToListAsync();

        var orders = await _context.Orders.ToListAsync();
        var postomats = await _context.Postomats.ToListAsync();

        var cells = new List<Cell>();
        foreach (var cellEntity in cellEntities)
        {
            var order = cellEntity.OrderId is not null
                ? orders.FirstOrDefault(o => o.Id == cellEntity.OrderId)
                : null;
            if (cellEntity.OrderId is not null && order is null)
            {
                await _context.Cells
                    .Where(c => c.Id == cellEntity.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(c => c.OrderId, c => null));

                throw new Exception($"Cell \"{cellEntity.Id}\" has unknown order id \"{cellEntity.OrderId}\" " +
                                    $"and was cleaned forcibly, repeat your request");
            }

            var postomat = postomats.FirstOrDefault(p => p.Id == cellEntity.PostomatId);
            if (postomat is null)
            {
                var numDeleted = await _context.Cells
                    .Where(c => c.PostomatId == cellEntity.PostomatId)
                    .ExecuteDeleteAsync();

                throw new Exception($"Cell \"{cellEntity.Id}\" has unknown postomat id \"{cellEntity.PostomatId}\" " +
                                    $"and all cells with destructive postomat id were deleted, " +
                                    $"{numDeleted} cells deleted");
            }

            var orderModel = order is not null
                ? Order.Create(order.Id, order.ReceivingCodeHash, order.OrderSize).Order
                : null;

            cells.Add(Cell
                .Create(
                    cellEntity.Id,
                    cellEntity.CellSize,
                    cellEntity.PostomatId,
                    orderModel)
                .Cell);
        }

        return cells;
    }

    public async Task<Guid> UpdateCell(Guid cellId, Cell newCell)
    {
        var oldCell = (await GetAllCells())
            .FirstOrDefault(c => c.Id == cellId);

        if (oldCell is null)
            throw new Exception($"Unknown cell id: \"{cellId}\"");

        if (oldCell.PostomatId != newCell.PostomatId)
            throw new Exception("You cannot change the postomat id");

        if (oldCell.Order is not null && newCell.CellSize != oldCell.CellSize)
            throw new Exception($"You cannot change the size of the cell \"{cellId}\"," +
                                $"because it contains an order \"{oldCell.Order.Id}\".");

        if (newCell.Order is not null && newCell.Order.OrderSize > newCell.CellSize)
            throw new Exception($"Cell \"{oldCell.Id}\"/\"{newCell.Id}\" size does not match " +
                                $"order \"{newCell.Order.Id}\" size");

        var orderId = newCell.Order?.Id;
        await _context.Cells
            .Where(c => c.Id == cellId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(c => c.CellSize, c => newCell.CellSize)
                .SetProperty(c => c.OrderId, c => orderId));

        return cellId;
    }

    public async Task<Guid> DeleteCell(Guid cellId)
    {
        var cell = (await GetAllCells())
            .FirstOrDefault(c => c.Id == cellId);

        if (cell is null)
            throw new Exception($"Unknown cell id: \"{cellId}\"");

        if (cell.Order is not null)
            throw new Exception($"Deleting a cell \"{cellId}\" is destructive, " +
                                $"it contains an order \"{cell.Order.Id}\"");

        await _context.Cells
            .Where(c => c.Id == cellId)
            .ExecuteDeleteAsync();

        return cellId;
    }
}