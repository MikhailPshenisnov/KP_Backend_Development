namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceDeleteLogResponse(
    Guid? DeletedLogId,
    string? ErrorMessage
);