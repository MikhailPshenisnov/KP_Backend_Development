namespace Postomat.API.Contracts.Responses;

public record GetFilteredUsersResponse(
    Guid Id,
    string Login,
    string PasswordHash,
    Guid RoleId
);