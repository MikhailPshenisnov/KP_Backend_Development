namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceGetLogRequest(
    Guid LogId
);