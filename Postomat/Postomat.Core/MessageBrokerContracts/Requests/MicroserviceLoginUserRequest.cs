namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceLoginUserRequest(
    string Login,
    string Password
);