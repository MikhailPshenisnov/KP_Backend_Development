namespace Postomat.API.Contracts.Requests;

public record DeliverOrderBackRequest(
    Guid PostomatId,
    string DeliveryCode
);