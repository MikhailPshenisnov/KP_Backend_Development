namespace Postomat.API.Contracts.Responses;

public record GetUserResponse(
    Guid Id,
    string Login,
    string PasswordHash,
    Guid RoleId
);