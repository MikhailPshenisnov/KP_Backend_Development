namespace Postomat.API.Contracts.Requests;

public record DeliverOrderRequest(
    Guid PostomatId,
    string DeliveryCode
);