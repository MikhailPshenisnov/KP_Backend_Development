using Postomat.Core.Models;

namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceUpdateLogRequest(
    Guid LogId,
    Log NewLog
);