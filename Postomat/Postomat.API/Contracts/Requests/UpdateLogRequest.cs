namespace Postomat.API.Contracts.Requests;

public record UpdateLogRequest(
    Guid LogId,
    DateTime NewLogDate,
    string NewLogOrigin,
    string NewLogType,
    string NewLogTitle,
    string NewLogMessage
);