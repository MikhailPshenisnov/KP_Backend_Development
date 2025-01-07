using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Exceptions.SpecificExceptions;
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
            throw new DestructiveActionException("You cannot create a cell with an order inside.");

        var postomatEntity = await _context.Postomats
            .FirstOrDefaultAsync(p => p.Id == cell.PostomatId);
        if (postomatEntity is null)
            throw new UnknownIdentifierException($"Unable to create cell \"{cell.Id}\", " +
                                                 $"it has an unknown postomat id \"{cell.PostomatId}\".");

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

        var postomatEntity = await _context.Postomats
            .FirstOrDefaultAsync(p => p.Id == postomatId);
        if (postomatEntity is null)
        {
            foreach (var cellEntity in cellEntities)
            {
                await _context.Cells
                    .Where(c => c.Id == cellEntity.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(c => c.OrderId, c => null));
                await _context.Cells
                    .Where(c => c.Id == cellEntity.Id)
                    .ExecuteDeleteAsync();
            }

            throw new UnknownIdentifierException($"Unable to get postomat \"{postomatId}\" cells, " +
                                                 $"postomat \"{postomatId}\" does not exist, " +
                                                 $"cells with that id where cleared and deleted forcibly.");
        }

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

                throw new UnknownIdentifierException($"Cell \"{cellEntity.Id}\" has unknown order id " +
                                                     $"\"{cellEntity.OrderId} and was cleaned forcibly.");
            }

            Order? orderModel = null;
            if (orderEntity is not null)
            {
                (orderModel, var orderError) = Order
                    .Create(
                        orderEntity.Id,
                        orderEntity.ReceivingCodeHash,
                        orderEntity.OrderSize);
                if (!string.IsNullOrEmpty(orderError))
                    throw new ConversionException($"Unable to convert order entity to order model. " +
                                                  $"--> {orderError}");
            }

            var (cellModel, cellError) = Cell
                .Create(
                    cellEntity.Id,
                    cellEntity.CellSize,
                    cellEntity.PostomatId,
                    orderModel);
            if (!string.IsNullOrEmpty(cellError))
                throw new ConversionException($"Unable to convert cell entity to cell model. " +
                                              $"--> {cellError}");

            cells.Add(cellModel);
        }

        return cells;
    }

    public async Task<Guid> UpdateCell(Guid cellId, Cell newCell)
    {
        var oldCellEntity = await _context.Cells
            .FirstOrDefaultAsync(c => c.Id == cellId);
        if (oldCellEntity is null)
            throw new UnknownIdentifierException($"Unknown cell id: \"{cellId}\".");

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

            throw new UnknownIdentifierException($"Cell \"{oldCellEntity.Id}\" has an unknown postomat id " +
                                                 $"\"{oldCellEntity.PostomatId}\" and was cleaned and " +
                                                 $"deleted forcibly.");
        }

        if (oldCellEntity.PostomatId != newCell.PostomatId)
            throw new DestructiveActionException("You cannot change the postomat id.");

        if (oldCellEntity.OrderId != newCell.Order?.Id &&
            oldCellEntity.OrderId is not null &&
            newCell.Order?.Id is not null)
            throw new DestructiveActionException("You cannot change the order in the cell, only fill or clear it.");

        if (oldCellEntity.OrderId is not null && newCell.CellSize != oldCellEntity.CellSize)
            throw new DestructiveActionException($"You cannot change the size of the cell \"{cellId}\"," +
                                                 $"because it contains an order \"{oldCellEntity.OrderId}\".");

        if (newCell.Order is not null && newCell.Order.OrderSize > newCell.CellSize)
            throw new DestructiveActionException($"Cell \"{oldCellEntity.Id}\"/\"{newCell.Id}\" size " +
                                                 $"does not match order \"{newCell.Order.Id}\" size.");

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

            throw new UnknownIdentifierException($"Cell \"{oldCellEntity.Id}\" has unknown order id " +
                                                 $"\"{oldCellEntity.OrderId} and was cleaned forcibly.");
        }

        var newCellOrderEntity = newCell.Order is not null
            ? orderEntities.FirstOrDefault(o => o.Id == oldCellEntity.OrderId)
            : null;
        if (newCell.Order is not null && newCellOrderEntity is null)
            throw new UnknownIdentifierException($"New cell \"{oldCellEntity.Id}\"/\"{newCell.Id}\" " +
                                                 $"has unknown order id.");

        if (newCellOrderEntity is not null)
        {
            var orderPlanEntities = await _context.OrderPlans.ToListAsync();
            var newCellOrderOrderPlanEntity = orderPlanEntities
                .FirstOrDefault(op => op.OrderId == newCellOrderEntity.Id && op.PostomatId == postomatEntity.Id);
            if (newCellOrderOrderPlanEntity is null)
            {
                throw new DestructiveActionException($"A discrepancy was detected between " +
                                                     $"the postomat and the order plan when " +
                                                     $"delivering the order {newCellOrderEntity.Id}.");
            }
        }

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
            throw new UnknownIdentifierException($"Unknown cell id: \"{cellId}\".");

        if (cell.OrderId is not null)
            throw new DestructiveActionException($"Deleting a cell \"{cellId}\" is destructive, " +
                                                 $"it contains an order \"{cell.OrderId}\".");

        await _context.Cells
            .Where(c => c.Id == cellId)
            .ExecuteDeleteAsync();

        return cellId;
    }
}