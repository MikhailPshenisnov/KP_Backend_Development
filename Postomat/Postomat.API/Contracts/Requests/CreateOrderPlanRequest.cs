namespace Postomat.API.Contracts.Requests;

public record CreateOrderPlanRequest(
    string DeliveryCode,
    Guid OrderId,
    Guid PostomatId
);