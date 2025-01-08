namespace Postomat.API.Contracts.Requests;

public record GetFilteredLogsRequest(
    DateTime? DateFrom,
    DateTime? DateTo,
    string? PartOfOrigin,
    string? PartOfType,
    string? PartOfTitle,
    string? PartOfMessage
);