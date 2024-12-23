namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceGetFilteredLogsResponse(
    List<LogDto>? LogDtoList,
    string? ErrorMessage
);