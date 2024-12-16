using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Application.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Guid> CreateUserAsync(User user, CancellationToken ct)
    {
        try
        {
            var createdUserId = await _usersRepository.CreateUser(user);

            return createdUserId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to create user \"{user.Id}\": \"{e.Message}\"");
        }
    }

    public async Task<User> GetUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var allUsers = await _usersRepository.GetAllUsers();

            var user = allUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new Exception($"Unknown user id: \"{userId}\"");

            return user;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to get user \"{userId}\": \"{e.Message}\"");
        }
    }

    public async Task<List<User>> GetFilteredUsersAsync(UserFilter? userFilter, CancellationToken ct)
    {
        try
        {
            var users = await _usersRepository.GetAllUsers();

            if (userFilter == null)
                return users;

            if (userFilter.PartOfLogin is not null)
            {
                users = users
                    .Where(u => u.Login.ToLower().Contains(userFilter.PartOfLogin.ToLower()))
                    .ToList();
            }

            if (userFilter.RoleId is not null)
            {
                users = users
                    .Where(u => u.Role.Id == userFilter.RoleId)
                    .ToList();
            }

            return users.OrderBy(x => x.Role.AccessLvl).ThenBy(x => x.Login).ToList();
        }

        catch (Exception e)
        {
            throw new Exception($"Unable to get filtered users: \"{e.Message}\"");
        }
    }

    public async Task<Guid> UpdateUserAsync(Guid userId, User newUser, CancellationToken ct)
    {
        try
        {
            var updatedUserId = await _usersRepository.UpdateUser(userId, newUser);

            return updatedUserId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to update user \"{userId}\": \"{e.Message}\"");
        }
    }

    public async Task<Guid> DeleteUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var existedUserId = await _usersRepository.DeleteUser(userId);

            return existedUserId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to delete user \"{userId}\": \"{e.Message}\"");
        }
    }
}