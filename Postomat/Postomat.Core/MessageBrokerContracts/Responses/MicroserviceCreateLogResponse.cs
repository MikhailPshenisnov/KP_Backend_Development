namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceCreateLogResponse(
    Guid? CreatedLogId,
    string? ErrorMessage
);