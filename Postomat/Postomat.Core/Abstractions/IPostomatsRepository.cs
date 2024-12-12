namespace Postomat.Core.Abstractions;

public interface IPostomatsRepository
{
    Task<Guid> CreatePostomat(Models.Postomat postomat);
    Task<List<Models.Postomat>> GetAllPostomats();
    Task<Guid> UpdatePostomat(Guid postomatId, Models.Postomat newPostomat);
    Task<Guid> DeletePostomat(Guid postomatId);
}