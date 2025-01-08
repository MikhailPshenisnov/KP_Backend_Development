namespace Postomat.API.Contracts.Requests;

public record CreateUserRequest(
    string Login,
    string Password,
    Guid RoleId
);