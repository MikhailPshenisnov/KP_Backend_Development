using Postomat.Core.Models.Filters;

namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceGetFilteredLogsRequest(
    LogFilter? LogFilter
);