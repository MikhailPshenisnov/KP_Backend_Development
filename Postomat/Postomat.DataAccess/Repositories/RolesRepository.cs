using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class RolesRepository : IRolesRepository
{
    private readonly PostomatDbContext _context;

    public RolesRepository(PostomatDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateRole(Role role)
    {
        if (role.AccessLvl == (int)AccessLvlEnumerator.SuperUser)
        {
            var superUserRole = (await GetAllRoles())
                .FirstOrDefault(r => r.AccessLvl == (int)AccessLvlEnumerator.SuperUser);
            if (superUserRole is not null)
                throw new Exception("The superuser role already exists");
        }

        var roleEntity = new Database.Entities.Role
        {
            Id = role.Id,
            RoleName = role.RoleName,
            AccessLvl = role.AccessLvl
        };

        await _context.Roles.AddAsync(roleEntity);
        await _context.SaveChangesAsync();

        return roleEntity.Id;
    }

    public async Task<List<Role>> GetAllRoles()
    {
        var roleEntities = await _context.Roles
            .AsNoTracking()
            .ToListAsync();

        var roles = roleEntities
            .Select(roleEntity => Role
                .Create(
                    roleEntity.Id,
                    roleEntity.RoleName,
                    roleEntity.AccessLvl)
                .Role)
            .ToList();

        return roles;
    }

    public async Task<Guid> UpdateRole(Guid roleId, Role newRole)
    {
        var allRoles = await GetAllRoles();

        var oldRole = allRoles
            .FirstOrDefault(r => r.Id == roleId);
        if (oldRole is null)
            throw new Exception($"Unknown role id: \"{roleId}\"");

        if (newRole.AccessLvl == (int)AccessLvlEnumerator.SuperUser)
        {
            var superUserRole = allRoles
                .FirstOrDefault(r => r.AccessLvl == (int)AccessLvlEnumerator.SuperUser);
            if (superUserRole is not null)
                throw new Exception("The superuser role already exists");
        }

        await _context.Roles
            .Where(r => r.Id == roleId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(r => r.RoleName, r => newRole.RoleName)
                .SetProperty(r => r.AccessLvl, r => newRole.AccessLvl));

        return roleId;
    }

    public async Task<Guid> DeleteRole(Guid roleId)
    {
        var userWithRole = await _context.Users
            .FirstOrDefaultAsync(user => user.Role.Id == roleId);
        if (userWithRole is not null)
            throw new Exception($"Deleting a role \"{roleId}\" is destructive, " +
                                $"user \"{userWithRole.Id}\" has this role");

        var role = (await GetAllRoles())
            .FirstOrDefault(r => r.Id == roleId);
        if (role is not null && role.AccessLvl == (int)AccessLvlEnumerator.SuperUser)
            throw new Exception("You cannot delete the superuser role");

        if (role is null)
            throw new Exception($"Unknown role id: \"{roleId}\"");

        await _context.Roles
            .Where(r => r.Id == roleId)
            .ExecuteDeleteAsync();

        return roleId;
    }
}