using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions.Repositories;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class PostomatsRepository : IPostomatsRepository
{
    private readonly PostomatDbContext _context;
    private readonly ICellsRepository _cellsRepository;

    public PostomatsRepository(PostomatDbContext context, ICellsRepository cellsRepository)
    {
        _context = context;
        _cellsRepository = cellsRepository;
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
            try
            {
                var postomatCells = await _cellsRepository.GetPostomatCells(postomatEntity.Id);

                postomats.Add(Core.Models.Postomat
                    .Create(
                        postomatEntity.Id,
                        postomatEntity.Name,
                        postomatEntity.Address,
                        postomatCells)
                    .Postomat);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred when getting the postomats: {e.Message}");
            }
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
            throw new Exception("You cannot change postomat cells from postomat" +
                                "repository, use cells repository for that");

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
        if (postomat is null)
            throw new Exception($"Unknown postomat id: \"{postomatId}\"");
        if (postomat.Cells.Count != 0)
            throw new Exception($"Deleting a postomat \"{postomatId}\" is destructive, " +
                                $"it contains cells, delete them first");

        var orderPlanWithPostomat = await _context.OrderPlans
            .FirstOrDefaultAsync(op => op.PostomatId == postomatId);
        if (orderPlanWithPostomat is not null)
            throw new Exception($"Deleting a postomat \"{postomatId}\" is destructive, " +
                                $"it is contained in an order plan \"{orderPlanWithPostomat.Id}\"");

        await _context.Postomats
            .Where(p => p.Id == postomatId)
            .ExecuteDeleteAsync();

        return postomatId;
    }
}