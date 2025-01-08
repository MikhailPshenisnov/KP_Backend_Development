namespace Postomat.API.Contracts.Requests;

public record UpdateUserRequest(
    Guid UserId,
    string Login,
    string Password,
    Guid RoleId
);