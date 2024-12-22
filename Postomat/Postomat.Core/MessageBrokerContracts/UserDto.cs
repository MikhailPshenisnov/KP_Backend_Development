namespace Postomat.Core.MessageBrokerContracts;

public record UserDto(
    Guid UserId,
    Guid RoleId
);