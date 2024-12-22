using Postomat.Core.Models;

namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceCreateLogRequest(
    Log Log
);