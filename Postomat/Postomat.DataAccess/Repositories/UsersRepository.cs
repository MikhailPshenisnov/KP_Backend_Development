using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.Models;
using Postomat.Core.Models.Other;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly PostomatDbContext _context;

    public UsersRepository(PostomatDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateUser(User user)
    {
        var existedUserEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.Login == user.Login);
        if (existedUserEntity is not null)
            throw new DestructiveActionException($"Unable to create user \"{user.Id}\", " +
                                                 $"login \"{user.Login}\" is already in use.");

        var roleEntity = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == user.Role.Id);
        if (roleEntity is null)
            throw new UnknownIdentifierException($"Unable to create user \"{user.Id}\", " +
                                                 $"it has an unknown role id \"{user.Role.Id}\".");

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
                if (baseRoleEntity is null)
                    throw new UnknownIdentifierException($"User \"{userEntity.Id}\" has unknown role id " +
                                                         $"\"{userEntity.RoleId}\", role has not been reset " +
                                                         $"to basic level, base user role does not exist.");

                await _context.Users
                    .Where(u => u.Id == userEntity.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(u => u.RoleId, u => baseRoleEntity.Id));
                throw new UnknownIdentifierException($"User \"{userEntity.Id}\" has unknown role id " +
                                                     $"\"{userEntity.RoleId}\", role has been reset " +
                                                     $"to basic level.");
            }

            var (roleModel, roleError) = Role
                .Create(
                    roleEntity.Id,
                    roleEntity.RoleName,
                    roleEntity.AccessLvl);
            if (!string.IsNullOrEmpty(roleError))
                throw new ConversionException($"Unable to convert role entity to role model. " +
                                              $"--> {roleError}");

            var (userModel, userError) = User
                .Create(
                    userEntity.Id,
                    userEntity.Login,
                    userEntity.PasswordHash,
                    roleModel);
            if (!string.IsNullOrEmpty(userError))
                throw new ConversionException($"Unable to convert user entity to user model. " +
                                              $"--> {userError}");

            users.Add(userModel);
        }

        return users;
    }

    public async Task<Guid> UpdateUser(Guid userId, User newUser)
    {
        var userEntities = await _context.Users.ToListAsync();
        var oldUserEntity = userEntities
            .FirstOrDefault(u => u.Id == userId);
        if (oldUserEntity is null)
            throw new UnknownIdentifierException($"Unknown user id: \"{userId}\".");
        var existedUserEntity = userEntities
            .FirstOrDefault(u => u.Id != oldUserEntity.Id && u.Login == newUser.Login);
        if (existedUserEntity is not null)
            throw new DestructiveActionException($"Unable to update user \"{userId}\", " +
                                                 $"login \"{newUser.Login}\" is already in use.");

        var roleEntities = await _context.Roles.ToListAsync();
        var oldUserRoleEntity = roleEntities.FirstOrDefault(r => r.Id == oldUserEntity.RoleId);
        if (oldUserRoleEntity is null)
        {
            var baseRoleEntity = await _context.Roles
                .FirstOrDefaultAsync(r => r.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee);
            if (baseRoleEntity is null)
                throw new UnknownIdentifierException($"User \"{oldUserEntity.Id}\" has unknown role id " +
                                                     $"\"{oldUserEntity.RoleId}\", role has not been reset " +
                                                     $"to basic level, base user role does not exist.");

            await _context.Users
                .Where(u => u.Id == oldUserEntity.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(u => u.RoleId, u => baseRoleEntity.Id));
            throw new UnknownIdentifierException($"User \"{oldUserEntity.Id}\" has unknown role id " +
                                                 $"\"{oldUserEntity.RoleId}\", role has been reset " +
                                                 $"to basic level.");
        }

        var newUserRoleEntity = roleEntities.FirstOrDefault(r => r.Id == newUser.Role.Id);
        if (newUserRoleEntity is null)
            throw new UnknownIdentifierException($"User \"{newUser.Id}\" has unknown role id \"{newUser.Role.Id}\".");

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
            throw new UnknownIdentifierException($"Unknown user id: \"{userId}\".");

        if (user.Role.AccessLvl == (int)AccessLvlEnumerator.SuperUser &&
            users.Where(u => u.Role.AccessLvl == (int)AccessLvlEnumerator.SuperUser).ToList().Count == 1)
            throw new DestructiveActionException("You cannot delete the last superuser.");

        var orderPlanWithUser = await _context.OrderPlans
            .FirstOrDefaultAsync(op => op.CreatedBy == userId ||
                                       op.DeliveredBy == userId ||
                                       op.DeliveredBackBy == userId);
        if (orderPlanWithUser is not null)
            throw new DestructiveActionException($"Deleting an user \"{userId}\" is destructive, " +
                                                 $"it is contained in an order plan \"{orderPlanWithUser.Id}\".");

        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync();

        return userId;
    }
}