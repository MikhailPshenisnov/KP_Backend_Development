namespace Postomat.API.Contracts.Responses;

public record GetFilteredLogsResponse(
    Guid Id,
    DateTime Date,
    string Origin,
    string Type,
    string Title,
    string Message
);