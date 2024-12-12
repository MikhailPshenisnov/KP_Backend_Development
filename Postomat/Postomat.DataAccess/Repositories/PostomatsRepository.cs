using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class PostomatsRepository : IPostomatsRepository
{
    private readonly PostomatDbContext _context;
    private readonly ICellsRepository _cellsRepository;
    private readonly IOrderPlansRepository _orderPlansRepository;

    public PostomatsRepository(PostomatDbContext context, ICellsRepository cellsRepository,
        IOrderPlansRepository orderPlansRepository)
    {
        _context = context;
        _cellsRepository = cellsRepository;
        _orderPlansRepository = orderPlansRepository;
    }

    public async Task<Guid> CreatePostomat(Core.Models.Postomat postomat)
    {
        if (postomat.Cells.Count != 0)
            throw new Exception("You cannot create a postomat with cells, only separately");

        var postomatEntity = new Database.Entities.Postomat
        {
            Id = postomat.Id,
            Name = postomat.Name,
            Address = postomat.Address
        };

        await _context.Postomats.AddAsync(postomatEntity);
        await _context.SaveChangesAsync();

        return postomatEntity.Id;
    }

    public async Task<List<Core.Models.Postomat>> GetAllPostomats()
    {
        var postomatEntities = await _context.Postomats
            .AsNoTracking()
            .ToListAsync();

        var postomats = new List<Core.Models.Postomat>();
        foreach (var postomatEntity in postomatEntities)
        {
            List<Cell> postomatCells;

            try
            {
                postomatCells = (await _cellsRepository.GetAllCells())
                    .Where(c => c.PostomatId == postomatEntity.Id)
                    .ToList();
            }
            catch (Exception e)
            {
                /* TODO */
                // Если в Cell сделать warn, то ошибка должна пропасть
                throw new Exception($"An error occurred when getting the postomat \"{postomatEntity.Id}\": " +
                                    $"{e.Message}");
            }

            postomats.Add(Core.Models.Postomat
                .Create(
                    postomatEntity.Id,
                    postomatEntity.Name,
                    postomatEntity.Address,
                    postomatCells)
                .Postomat);
        }

        return postomats;
    }

    public async Task<Guid> UpdatePostomat(Guid postomatId, Core.Models.Postomat newPostomat)
    {
        var oldPostomat = (await GetAllPostomats())
            .FirstOrDefault(p => p.Id == postomatId);

        if (oldPostomat is null)
            throw new Exception($"Unknown postomat id: \"{postomatId}\"");

        if (oldPostomat.Cells != newPostomat.Cells)
            throw new Exception("You cannot change postomat cells from here, " +
                                "use cells repository for that");

        await _context.Postomats
            .Where(p => p.Id == postomatId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(p => p.Name, p => newPostomat.Name)
                .SetProperty(p => p.Address, p => newPostomat.Address));

        return postomatId;
    }

    public async Task<Guid> DeletePostomat(Guid postomatId)
    {
        var postomat = (await GetAllPostomats())
            .FirstOrDefault(p => p.Id == postomatId);
        if (postomat?.Cells.Count != 0)
            throw new Exception($"Deleting a postomat \"{postomatId}\" is destructive, " +
                                $"it contains cells, delete them first");

        var orderPlanWithPostomat = (await _orderPlansRepository.GetAllOrderPlans())
            .FirstOrDefault(orderPlan => orderPlan.Postomat.Id == postomatId);
        if (orderPlanWithPostomat is not null)
            throw new Exception($"Deleting a postomat \"{postomatId}\" is destructive, " +
                                $"it is contained in an order plan \"{orderPlanWithPostomat.Id}\"");


        var numUpdated = await _context.Postomats
            .Where(p => p.Id == postomatId)
            .ExecuteDeleteAsync();

        if (numUpdated == 0)
        {
            throw new Exception($"Unknown postomat id: \"{postomatId}\"");
        }

        return postomatId;
    }
}