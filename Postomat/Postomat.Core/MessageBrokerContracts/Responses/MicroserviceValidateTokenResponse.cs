namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceValidateTokenResponse(
    bool IsValid,
    UserDto? UserDto,
    string? Message,
    string? ErrorMessage
);