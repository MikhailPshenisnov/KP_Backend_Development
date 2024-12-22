namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceUpdateLogResponse(
    Guid? UpdatedLogId,
    string? ErrorMessage
);