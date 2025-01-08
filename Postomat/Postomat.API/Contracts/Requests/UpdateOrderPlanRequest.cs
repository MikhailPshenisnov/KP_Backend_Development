namespace Postomat.API.Contracts.Requests;

public record UpdateOrderPlanRequest(
    Guid OrderPlanId,
    string NewOrderPlanDeliveryCode
);