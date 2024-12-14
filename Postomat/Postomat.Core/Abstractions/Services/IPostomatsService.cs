using Postomat.Core.Models.Filters;
using Postomat.Core.Models.Filtres;

namespace Postomat.Core.Abstractions.Services;

public interface IPostomatsService
{
    Task<Guid> CreatePostomatAsync(Models.Postomat postomat, CancellationToken ct);
    Task<Models.Postomat> GetPostomatAsync(Guid postomatId, CancellationToken ct);
    Task<List<Models.Postomat>> GetFilteredPostomatsAsync(PostomatFilter? postomatFilter, CancellationToken ct);
    Task<Guid> UpdatePostomatAsync(Guid postomatId, Models.Postomat newPostomat, CancellationToken ct);
    Task<Guid> DeletePostomatAsync(Guid postomatId, CancellationToken ct);
}