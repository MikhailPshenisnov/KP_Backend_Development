using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions.RepositoryExceptions;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Application.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _rolesRepository;

    public RolesService(IRolesRepository rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }

    public async Task<Guid> CreateRoleAsync(Role role, CancellationToken ct)
    {
        try
        {
            var createdRoleId = await _rolesRepository.CreateRole(role);

            return createdRoleId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to create role \"{role.Id}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Role> GetRoleAsync(Guid roleId, CancellationToken ct)
    {
        try
        {
            var allRoles = await _rolesRepository.GetAllRoles();

            var role = allRoles.FirstOrDefault(r => r.Id == roleId);
            if (role == null)
                throw new UnknownIdentifierException($"Unknown role id: \"{roleId}\".");

            return role;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to get role \"{roleId}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<List<Role>> GetFilteredRolesAsync(RoleFilter? roleFilter, CancellationToken ct)
    {
        try
        {
            var roles = await _rolesRepository.GetAllRoles();

            if (roleFilter == null)
                return roles;

            if (roleFilter.PartOfRoleName is not null)
            {
                roles = roles
                    .Where(r => r.RoleName.ToLower().Contains(roleFilter.PartOfRoleName.ToLower()))
                    .ToList();
            }

            if (roleFilter.AccessLvlFrom is not null)
            {
                roles = roles
                    .Where(r => r.AccessLvl >= roleFilter.AccessLvlFrom)
                    .ToList();
            }

            if (roleFilter.AccessLvlTo is not null)
            {
                roles = roles
                    .Where(r => r.AccessLvl <= roleFilter.AccessLvlTo)
                    .ToList();
            }

            return roles.OrderBy(x => x.AccessLvl).ThenBy(x => x.RoleName).ToList();
        }

        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to get filtered roles. " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> UpdateRoleAsync(Guid roleId, Role newRole, CancellationToken ct)
    {
        try
        {
            var updatedRoleId = await _rolesRepository.UpdateRole(roleId, newRole);

            return updatedRoleId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to update role \"{roleId}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> DeleteRoleAsync(Guid roleId, CancellationToken ct)
    {
        try
        {
            var existedRoleId = await _rolesRepository.DeleteRole(roleId);

            return existedRoleId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to delete role \"{roleId}\". " +
                                       $"--> {e.Message}");
        }
    }
}