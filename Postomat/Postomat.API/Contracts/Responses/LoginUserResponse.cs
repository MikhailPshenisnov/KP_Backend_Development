namespace Postomat.API.Contracts.Responses;

public record LoginUserResponse(
    string Message,
    string Token
);