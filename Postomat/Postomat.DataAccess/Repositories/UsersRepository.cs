using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

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
        var existedUserEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.Login == user.Login);
        if (existedUserEntity is not null)
            throw new Exception($"Unable to create user \"{user.Id}\", login \"{user.Login}\" is already in use");

        var roleEntity = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == user.Role.Id);
        if (roleEntity is null)
            throw new Exception($"Unable to create user \"{user.Id}\", it has an unknown role id \"{user.Role.Id}\"");

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

        var roleEntities = await _context.Roles.ToListAsync();

        var users = new List<User>();
        foreach (var userEntity in userEntities)
        {
            var roleEntity = roleEntities.FirstOrDefault(r => r.Id == userEntity.RoleId);
            if (roleEntity is null)
            {
                var baseRoleEntity = await _context.Roles
                    .FirstOrDefaultAsync(r => r.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee);
                await _context.Users
                    .Where(u => u.Id == userEntity.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(u => u.RoleId, u => baseRoleEntity!.Id));
                throw new Exception($"User \"{userEntity.Id}\" has unknown role id \"{userEntity.RoleId}\"," +
                                    $"role has been reset to basic level");
            }

            var roleModel = Role.Create(roleEntity.Id, roleEntity.RoleName, roleEntity.AccessLvl).Role;

            users.Add(User
                .Create(
                    userEntity.Id,
                    userEntity.Login,
                    userEntity.PasswordHash,
                    roleModel)
                .User);
        }

        return users;
    }

    public async Task<Guid> UpdateUser(Guid userId, User newUser)
    {
        var userEntities = await _context.Users.ToListAsync();
        var oldUserEntity = userEntities
            .FirstOrDefault(u => u.Id == userId);
        if (oldUserEntity is null)
            throw new Exception($"Unknown user id: \"{userId}\"");
        var existedUserEntity = userEntities
            .FirstOrDefault(u => u.Id != oldUserEntity.Id && u.Login == newUser.Login);
        if (existedUserEntity is not null)
            throw new Exception($"Unable to update user \"{userId}\", login \"{newUser.Login}\" is already in use");

        var roleEntities = await _context.Roles.ToListAsync();
        var oldUserRoleEntity = roleEntities.FirstOrDefault(r => r.Id == oldUserEntity.RoleId);
        if (oldUserRoleEntity is null)
        {
            var baseRoleEntity = await _context.Roles
                .FirstOrDefaultAsync(r => r.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee);
            await _context.Users
                .Where(u => u.Id == oldUserEntity.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(u => u.RoleId, u => baseRoleEntity!.Id));
            throw new Exception($"User \"{oldUserEntity.Id}\" has unknown role id \"{oldUserEntity.RoleId}\"," +
                                $"role has been reset to basic level");
        }

        var newUserRoleEntity = roleEntities.FirstOrDefault(r => r.Id == newUser.Role.Id);
        if (newUserRoleEntity is null)
            throw new Exception($"User \"{newUser.Id}\" has unknown role id \"{newUser.Role.Id}\"");

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
        var users = await GetAllUsers();
        var user = users
            .FirstOrDefault(u => u.Id == userId);

        if (user is null)
            throw new Exception($"Unknown user id: \"{userId}\"");

        if (user.Role.AccessLvl == (int)AccessLvlEnumerator.SuperUser)
            throw new Exception("You cannot delete the superuser");

        if (user.Role.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee &&
            (users.Where(u => u.Role.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee).ToList()).Count == 1)
        {
            throw new Exception("You cannot delete last base user role");
        }

        var orderPlanWithUser = await _context.OrderPlans
            .FirstOrDefaultAsync(op => op.CreatedBy == userId ||
                                       op.DeliveredBy == userId ||
                                       op.DeliveredBackBy == userId);
        if (orderPlanWithUser is not null)
            throw new Exception($"Deleting an user \"{userId}\" is destructive, " +
                                $"it is contained in an order plan \"{orderPlanWithUser.Id}\"");

        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync();

        return userId;
    }
}