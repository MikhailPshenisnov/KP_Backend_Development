namespace Postomat.API.Contracts.Responses;

public record ValidateTokenResponse(
    Guid UserId,
    Guid RoleId
);