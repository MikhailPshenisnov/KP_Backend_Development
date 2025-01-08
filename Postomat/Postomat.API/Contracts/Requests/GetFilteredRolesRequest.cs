namespace Postomat.API.Contracts.Requests;

public record GetFilteredRolesRequest(
    string? PartOfRoleName,
    int? AccessLvlFrom,
    int? AccessLvlTo
);