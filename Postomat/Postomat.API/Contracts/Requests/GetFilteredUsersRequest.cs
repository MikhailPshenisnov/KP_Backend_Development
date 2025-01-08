namespace Postomat.API.Contracts.Requests;

public record GetFilteredUsersRequest(
    string? PartOfLogin,
    Guid? RoleId
);