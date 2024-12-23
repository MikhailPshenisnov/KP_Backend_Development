namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceGetLogResponse(
    LogDto? LogDto,
    string? ErrorMessage
);