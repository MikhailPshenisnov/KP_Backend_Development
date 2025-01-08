namespace Postomat.API.Contracts.Requests;

public record DeleteOrderPlanRequest(
    Guid OrderPlanId
);