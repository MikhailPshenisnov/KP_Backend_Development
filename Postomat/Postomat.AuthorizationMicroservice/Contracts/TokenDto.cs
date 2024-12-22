namespace Postomat.AuthorizationMicroservice.Contracts;

public record TokenDto(
    Guid UserId,
    Guid RoleId
);