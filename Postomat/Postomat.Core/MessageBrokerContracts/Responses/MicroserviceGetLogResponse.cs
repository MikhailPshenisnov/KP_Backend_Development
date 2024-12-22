using Postomat.Core.Models;

namespace Postomat.Core.MessageBrokerContracts.Responses;

public record MicroserviceGetLogResponse(
    Log? Log,
    string? ErrorMessage
);