namespace Postomat.API.Contracts.Responses;

public record GetLogResponse(
    Guid Id,
    DateTime Date,
    string Origin,
    string Type,
    string Title,
    string Message
);