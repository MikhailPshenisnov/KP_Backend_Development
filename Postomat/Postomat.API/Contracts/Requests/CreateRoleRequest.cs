namespace Postomat.API.Contracts.Requests;

public record CreateRoleRequest(
    string RoleName,
    int AccessLvl
);