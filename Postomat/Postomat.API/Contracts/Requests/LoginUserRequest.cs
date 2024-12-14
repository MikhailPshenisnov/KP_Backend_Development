namespace Postomat.API.Contracts.Requests;

public record LoginUserRequest(
    string Login,
    string Password
);