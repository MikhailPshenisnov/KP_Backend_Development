namespace Postomat.API.Contracts.Requests;

public record UpdateRoleRequest(
    Guid RoleId,
    string NewRoleRoleName,
    int NewRoleAccessLvl
);