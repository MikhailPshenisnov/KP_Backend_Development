using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Models;
using Postomat.Core.Models.Other;
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
            var superUserRoleEntity = await _context.Roles
                .FirstOrDefaultAsync(r => r.AccessLvl == (int)AccessLvlEnumerator.SuperUser);
            if (superUserRoleEntity is not null)
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
        var allRoleEntities = await _context.Roles.ToListAsync();

        var oldRoleEntity = allRoleEntities
            .FirstOrDefault(r => r.Id == roleId);
        if (oldRoleEntity is null)
            throw new Exception($"Unknown role id: \"{roleId}\"");

        if (oldRoleEntity.AccessLvl == (int)AccessLvlEnumerator.SuperUser &&
            newRole.AccessLvl != oldRoleEntity.AccessLvl)
            throw new Exception("You cannot change the superuser access level");
        
        if (oldRoleEntity.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee &&
            newRole.AccessLvl != oldRoleEntity.AccessLvl &&
            allRoleEntities
                .Where(r => r.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee)
                .ToList().Count == 1)
            throw new Exception("You cannot change last base user role");

        if (oldRoleEntity.AccessLvl != (int)AccessLvlEnumerator.SuperUser &&
            newRole.AccessLvl == (int)AccessLvlEnumerator.SuperUser)
        {
            var superUserRoleEntity = allRoleEntities
                .FirstOrDefault(r => r.AccessLvl == (int)AccessLvlEnumerator.SuperUser);
            if (superUserRoleEntity is not null)
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
        var userWithRoleEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.Role.Id == roleId);
        if (userWithRoleEntity is not null)
            throw new Exception($"Deleting a role \"{roleId}\" is destructive, " +
                                $"user \"{userWithRoleEntity.Id}\" has this role");

        var allRoleEntities = await _context.Roles.ToListAsync();
        var roleEntity = allRoleEntities
            .FirstOrDefault(r => r.Id == roleId);
        if (roleEntity is null)
            throw new Exception($"Unknown role id: \"{roleId}\"");
        if (roleEntity.AccessLvl == (int)AccessLvlEnumerator.SuperUser)
            throw new Exception("You cannot delete the superuser role");
        if (roleEntity.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee &&
            allRoleEntities
                .Where(r => r.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee)
                .ToList().Count == 1)
            throw new Exception("You cannot delete last base user role");

        await _context.Roles
            .Where(r => r.Id == roleId)
            .ExecuteDeleteAsync();

        return roleId;
    }
}