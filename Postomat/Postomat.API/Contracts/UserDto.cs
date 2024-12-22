namespace Postomat.API.Contracts;

public record UserDto
(
    Guid UserId,
    Guid RoleId
);