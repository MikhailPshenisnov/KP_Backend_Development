namespace Postomat.API.Contracts.Requests;

public record CreateLogRequest(
    DateTime Date,
    string Origin,
    string Type,
    string Title,
    string Message
);