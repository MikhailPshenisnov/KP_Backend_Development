namespace Postomat.Core.MessageBrokerContracts;

public record LogFilterDto(
    DateTime? DateFrom,
    DateTime? DateTo,
    string? PartOfOrigin,
    string? PartOfType,
    string? PartOfTitle,
    string? PartOfMessage
);