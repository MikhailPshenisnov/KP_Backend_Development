using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Abstractions.Repositories;
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

        var postomatEntity = await _context.Postomats
            .FirstOrDefaultAsync(p => p.Id == cell.PostomatId);
        if (postomatEntity is null)
            throw new Exception($"Unable to create cell \"{cell.Id}\", " +
                                $"it has an unknown postomat id \"{cell.PostomatId}\"");

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

    public async Task<List<Cell>> GetPostomatCells(Guid postomatId)
    {
        var cellEntities = await _context.Cells
            .Where(c => c.PostomatId == postomatId)
            .AsNoTracking()
            .ToListAsync();

        var cells = new List<Cell>();

        if (cellEntities.Count == 0)
            return cells;

        var orderEntities = await _context.Orders.ToListAsync();

        foreach (var cellEntity in cellEntities)
        {
            var orderEntity = cellEntity.OrderId is not null
                ? orderEntities.FirstOrDefault(o => o.Id == cellEntity.OrderId)
                : null;
            if (cellEntity.OrderId is not null && orderEntity is null)
            {
                await _context.Cells
                    .Where(c => c.Id == cellEntity.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(c => c.OrderId, c => null));

                throw new Exception($"Cell \"{cellEntity.Id}\" has unknown order id " +
                                    $"\"{cellEntity.OrderId} and was cleaned forcibly");
            }

            var orderModel = orderEntity is not null
                ? Order.Create(orderEntity.Id, orderEntity.ReceivingCodeHash, orderEntity.OrderSize).Order
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
        var oldCellEntity = await _context.Cells
            .FirstOrDefaultAsync(c => c.Id == cellId);
        if (oldCellEntity is null)
            throw new Exception($"Unknown cell id: \"{cellId}\"");

        var postomatEntity = await _context.Postomats
            .FirstOrDefaultAsync(p => p.Id == oldCellEntity.PostomatId);
        if (postomatEntity is null)
        {
            await _context.Cells
                .Where(c => c.Id == oldCellEntity.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(c => c.OrderId, c => null));

            await _context.Cells
                .Where(c => c.Id == oldCellEntity.Id)
                .ExecuteDeleteAsync();

            throw new Exception($"Cell \"{oldCellEntity.Id}\" has an unknown postomat id " +
                                $"\"{oldCellEntity.PostomatId}\" and was cleaned forcibly and deleted");
        }

        if (oldCellEntity.PostomatId != newCell.PostomatId)
            throw new Exception("You cannot change the postomat id");

        if (oldCellEntity.OrderId != newCell.Order?.Id &&
            oldCellEntity.OrderId is not null &&
            newCell.Order?.Id is not null)
            throw new Exception("You cannot change the order in the cell, only fill or clear it");

        if (oldCellEntity.OrderId is not null && newCell.CellSize != oldCellEntity.CellSize)
            throw new Exception($"You cannot change the size of the cell \"{cellId}\"," +
                                $"because it contains an order \"{oldCellEntity.OrderId}\".");

        if (newCell.Order is not null && newCell.Order.OrderSize > newCell.CellSize)
            throw new Exception($"Cell \"{oldCellEntity.Id}\"/\"{newCell.Id}\" size does not match " +
                                $"order \"{newCell.Order.Id}\" size");

        var orderEntities = await _context.Orders.ToListAsync();

        var oldCellOrderEntity = oldCellEntity.OrderId is not null
            ? orderEntities.FirstOrDefault(o => o.Id == oldCellEntity.OrderId)
            : null;
        if (oldCellEntity.OrderId is not null && oldCellOrderEntity is null)
        {
            await _context.Cells
                .Where(c => c.Id == oldCellEntity.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(c => c.OrderId, c => null));

            throw new Exception($"Cell \"{oldCellEntity.Id}\" has unknown order id " +
                                $"\"{oldCellEntity.OrderId} and was cleaned forcibly");
        }

        var newCellOrderEntity = newCell.Order is not null
            ? orderEntities.FirstOrDefault(o => o.Id == oldCellEntity.OrderId)
            : null;
        if (newCell.Order is not null && newCellOrderEntity is null)
            throw new Exception($"New cell \"{oldCellEntity.Id}\"/\"{newCell.Id}\" has unknown order id");

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
        var cell = await _context.Cells
            .FirstOrDefaultAsync(c => c.Id == cellId);

        if (cell is null)
            throw new Exception($"Unknown cell id: \"{cellId}\"");

        if (cell.OrderId is not null)
            throw new Exception($"Deleting a cell \"{cellId}\" is destructive, " +
                                $"it contains an order \"{cell.OrderId}\"");

        await _context.Cells
            .Where(c => c.Id == cellId)
            .ExecuteDeleteAsync();

        return cellId;
    }
}