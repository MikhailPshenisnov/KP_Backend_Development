using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Core.Abstractions.Services;

public interface IPostomatsService
{
    Task<Guid> CreatePostomatAsync(Models.Postomat postomat, CancellationToken ct);
    Task<Models.Postomat> GetPostomatAsync(Guid postomatId, CancellationToken ct);
    Task<List<Models.Postomat>> GetFilteredPostomatsAsync(PostomatFilter? postomatFilter, CancellationToken ct);
    Task<Guid> UpdatePostomatAsync(Guid postomatId, Models.Postomat newPostomat, CancellationToken ct);
    Task<Guid> DeletePostomatAsync(Guid postomatId, CancellationToken ct);

    Task<Guid> AddCellToPostomatAsync(Cell cell, CancellationToken ct);
    Task<(Guid CellId, Guid OrderPlanId)> FillCellInPostomatAsync(User user, Guid postomatId, Order order, 
        CancellationToken ct);
    Task<(Guid CellId, Guid OrderPlanId)> ClearCellInPostomatAsync(User? user, Guid postomatId, Order order, 
        CancellationToken ct);
    Task<Guid> DeleteCellFromPostomatAsync(Guid cellId, CancellationToken ct);
}