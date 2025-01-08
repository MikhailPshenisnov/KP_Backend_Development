namespace Postomat.API.Contracts.Responses;

public record GetFilteredRolesResponse(
    Guid Id,
    string RoleName,
    int AccessLvl
);