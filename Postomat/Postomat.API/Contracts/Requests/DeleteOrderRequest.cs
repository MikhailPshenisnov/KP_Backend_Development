namespace Postomat.API.Contracts.Requests;

public record DeleteOrderRequest(
    Guid OrderId
);