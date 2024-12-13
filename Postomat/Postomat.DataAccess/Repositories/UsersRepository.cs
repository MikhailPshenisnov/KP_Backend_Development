using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

/* TODO */
public class UsersRepository : IUsersRepository
{
    private readonly PostomatDbContext _context;
    private readonly IRolesRepository _rolesRepository;

    public UsersRepository(PostomatDbContext context, IRolesRepository rolesRepository)
    {
        _context = context;
        _rolesRepository = rolesRepository;
    }

    public async Task<Guid> CreateUser(User user)
    {
        var userEntity = new Database.Entities.User
        {
            Id = user.Id,
            Login = user.Login,
            PasswordHash = user.PasswordHash,
            RoleId = user.Role.Id
        };

        await _context.Users.AddAsync(userEntity);
        await _context.SaveChangesAsync();

        return userEntity.Id;
    }

    public async Task<List<User>> GetAllUsers()
    {
        var userEntities = await _context.Users
            .AsNoTracking()
            .ToListAsync();

        var users = new List<User>();
        foreach (var userEntity in userEntities)
        {
            var role = (await _rolesRepository.GetAllRoles())
                .FirstOrDefault(r => r.Id == userEntity.RoleId);

            if (role is null)
                throw new Exception($"User \"{userEntity.Id}\" has unknown role id");

            users.Add(User
                .Create(
                    userEntity.Id,
                    userEntity.Login,
                    userEntity.PasswordHash,
                    role)
                .User);
        }

        return users;
    }

    public async Task<Guid> UpdateUser(Guid userId, User newUser)
    {
        var oldUser = (await GetAllUsers())
            .FirstOrDefault(u => u.Id == userId);

        if (oldUser is null)
            throw new Exception("Unknown user id");

        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(u => u.Login, u => newUser.Login)
                .SetProperty(u => u.PasswordHash, u => newUser.PasswordHash)
                .SetProperty(u => u.RoleId, u => newUser.Role.Id));

        return userId;
    }

    public async Task<Guid> DeleteUser(Guid userId)
    {
        var numUpdated = await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync();

        if (numUpdated == 0)
        {
            throw new Exception("Unknown user id");
        }

        return userId;
    }
}