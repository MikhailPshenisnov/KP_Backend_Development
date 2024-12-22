namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceLoginUserResponse(
    bool IsSuccess,
    string? Token,
    string? ErrorMessage
);