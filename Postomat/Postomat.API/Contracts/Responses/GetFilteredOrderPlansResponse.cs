namespace Postomat.API.Contracts.Responses;

public record GetFilteredOrderPlansResponse(
    Guid Id,
    string Status,
    DateTime LastStatusChangeDate,
    DateTime? StoreUntilDate,
    string DeliveryCodeHash,
    Guid OrderId,
    Guid PostomatId,
    Guid CreatedBy,
    Guid? DeliveredBy,
    Guid? DeliveredBackBy
);