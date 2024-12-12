using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class CellsRepository : ICellsRepository
{
    private readonly PostomatDbContext _context;
    private readonly IOrdersRepository _ordersRepository;

    public CellsRepository(PostomatDbContext context, IOrdersRepository ordersRepository)
    {
        _context = context;
        _ordersRepository = ordersRepository;
    }

    public async Task<Guid> CreateCell(Cell cell)
    {
        if (cell.Order is not null)
            throw new Exception("You cannot create a cell with an order inside");

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

        var cells = new List<Cell>();
        foreach (var cellEntity in cellEntities)
        {
            var order = cellEntity.OrderId is not null
                ? (await _ordersRepository.GetAllOrders()).FirstOrDefault(o => o.Id == cellEntity.OrderId)
                : null;

            if (cellEntity.OrderId is not null && order is null)
            {
                await _context.Cells
                    .Where(c => c.Id == cellEntity.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(c => c.OrderId, c => null));

                /* TODO */
                // Можно сделать warn и выполнить функцию до конца
                throw new Exception($"Cell \"{cellEntity.Id}\" has unknown order id \"{cellEntity.OrderId}\" " +
                                    $"and was cleaned forcibly, repeat your request");
            }

            cells.Add(Cell
                .Create(
                    cellEntity.Id,
                    cellEntity.CellSize,
                    cellEntity.PostomatId,
                    order)
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
        if (cell?.Order is not null)
            throw new Exception($"Deleting a cell \"{cellId}\" is destructive, " +
                                $"it contains an order \"{cell.Order.Id}\"");

        var numUpdated = await _context.Cells
            .Where(c => c.Id == cellId)
            .ExecuteDeleteAsync();

        if (numUpdated == 0)
            throw new Exception($"Unknown cell id: \"{cellId}\"");

        return cellId;
    }
}