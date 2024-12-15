using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Core.Abstractions.Services;

public interface IRolesService
{
    Task<Guid> CreateRoleAsync(Role role, CancellationToken ct);
    Task<Role> GetRoleAsync(Guid roleId, CancellationToken ct);
    Task<List<Role>> GetFilteredRolesAsync(RoleFilter? roleFilter, CancellationToken ct);
    Task<Guid> UpdateRoleAsync(Guid roleId, Role newRole, CancellationToken ct);
    Task<Guid> DeleteRoleAsync(Guid roleId, CancellationToken ct);
}