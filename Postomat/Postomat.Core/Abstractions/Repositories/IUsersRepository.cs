using Postomat.Core.Models;

namespace Postomat.Core.Abstractions.Repositories;

public interface IUsersRepository
{
    Task<Guid> CreateUser(User user);
    Task<List<User>> GetAllUsers();
    Task<Guid> UpdateUser(Guid userId, User newUser);
    Task<Guid> DeleteUser(Guid userId);
}