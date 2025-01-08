namespace Postomat.API.Contracts.Responses;

public record GetRoleResponse(
    Guid Id,
    string RoleName,
    int AccessLvl
);