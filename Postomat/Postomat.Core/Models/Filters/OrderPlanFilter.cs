namespace Postomat.Core.Models.Filtres;

public class OrderPlanFilter
{
    private OrderPlanFilter(string? partOfStatus, DateTime? lastStatusChangeDateFrom, DateTime? lastStatusChangeDateTo,
        DateTime? storeUntilDateFrom, DateTime? storeUntilDateTo, Guid? orderId, Guid? postomatId, Guid? userId)
    {
        PartOfStatus = partOfStatus;
        LastStatusChangeDateFrom = lastStatusChangeDateFrom;
        LastStatusChangeDateTo = lastStatusChangeDateTo;
        StoreUntilDateFrom = storeUntilDateFrom;
        StoreUntilDateTo = storeUntilDateTo;
        OrderId = orderId;
        PostomatId = postomatId;
        UserId = userId;
    }

    public string? PartOfStatus { get; }
    public DateTime? LastStatusChangeDateFrom { get; }
    public DateTime? LastStatusChangeDateTo { get; }
    public DateTime? StoreUntilDateFrom { get; }
    public DateTime? StoreUntilDateTo { get; }
    public Guid? OrderId { get; }
    public Guid? PostomatId { get; }
    public Guid? UserId { get; }

    private static string BasicChecks(DateTime? lastStatusChangeDateFrom, DateTime? lastStatusChangeDateTo,
        DateTime? storeUntilDateFrom, DateTime? storeUntilDateTo)
    {
        var error = string.Empty;

        if (lastStatusChangeDateFrom is not null && lastStatusChangeDateFrom > DateTime.Now)
        {
            error = "Last status change date from can't be from the future";
        }
        else if (lastStatusChangeDateFrom is not null &&
                 lastStatusChangeDateTo is not null &&
                 lastStatusChangeDateFrom > lastStatusChangeDateTo)
        {
            error = "Wrong order of last status change date from and last status change date to";
        }
        else if (storeUntilDateFrom is not null && storeUntilDateFrom > DateTime.Now)
        {
            error = "Store until date from can't be from the future";
        }
        else if (storeUntilDateFrom is not null &&
                 storeUntilDateTo is not null &&
                 storeUntilDateFrom > storeUntilDateTo)
        {
            error = "Wrong order of store until date from and store until date to";
        }

        return error;
    }

    public static (OrderPlanFilter OrderPlanFilter, string Error) Create(string? partOfStatus,
        DateTime? lastStatusChangeDateFrom, DateTime? lastStatusChangeDateTo, DateTime? storeUntilDateFrom,
        DateTime? storeUntilDateTo, Guid? orderId, Guid? postomatId, Guid? userId)
    {
        var error = BasicChecks(lastStatusChangeDateFrom, lastStatusChangeDateTo, storeUntilDateFrom,
            storeUntilDateTo);

        var orderPlanFilter = new OrderPlanFilter(partOfStatus, lastStatusChangeDateFrom, lastStatusChangeDateTo,
            storeUntilDateFrom, storeUntilDateTo, orderId, postomatId, userId);

        return (orderPlanFilter, error);
    }
}