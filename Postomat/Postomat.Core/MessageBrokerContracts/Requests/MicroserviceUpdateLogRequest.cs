namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceUpdateLogRequest(
    Guid LogId,
    LogDto NewLogDto
);