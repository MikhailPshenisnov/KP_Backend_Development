namespace Postomat.Core.MessageBrokerContracts;

public record LogDto(
    Guid Id,
    DateTime Date,
    string Origin,
    string Type,
    string Title,
    string Message
);