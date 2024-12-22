using Postomat.Core.Models;

namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceGetFilteredLogsResponse(
    List<Log>? Logs,
    string? ErrorMessage
);