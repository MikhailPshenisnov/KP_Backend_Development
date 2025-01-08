namespace Postomat.API.Contracts.Requests;

public record GetFilteredOrderPlansRequest(
    string? PartOfStatus,
    DateTime? LastStatusChangeDateFrom,
    DateTime? LastStatusChangeDateTo,
    DateTime? StoreUntilDateFrom,
    DateTime? StoreUntilDateTo,
    Guid? OrderId,
    Guid? PostomatId,
    Guid? UserId
);