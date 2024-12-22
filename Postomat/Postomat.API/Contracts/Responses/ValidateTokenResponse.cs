namespace Postomat.API.Contracts.Responses;

public record ValidateTokenResponse(
    string Message,
    Guid UserId,
    Guid RoleId
);