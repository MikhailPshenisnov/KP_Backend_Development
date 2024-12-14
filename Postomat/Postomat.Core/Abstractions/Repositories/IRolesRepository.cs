using Postomat.Core.Models;

namespace Postomat.Core.Abstractions.Repositories;

public interface IRolesRepository
{
    Task<Guid> CreateRole(Role role);
    Task<List<Role>> GetAllRoles();
    Task<Guid> UpdateRole(Guid roleId, Role newRole);
    Task<Guid> DeleteRole(Guid roleId);
}