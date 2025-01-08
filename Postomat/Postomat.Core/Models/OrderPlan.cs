namespace Postomat.Core.Models;

public class OrderPlan
{
    public const int MaxStatusLength = 128;
    public const int MaxDeliveryCodeHashLength = 128;
    public static readonly List<string> AllowedStatuses = ["Created", "Delivered", "Finished"];

    private OrderPlan(Guid id, string status, DateTime lastStatusChangeDate, DateTime? storeUntilDate,
        string deliveryCodeHash, Order order, Postomat postomat, User createdBy, User? deliveredBy,
        User? deliveredBackBy)
    {
        Id = id;
        Status = status;
        LastStatusChangeDate = lastStatusChangeDate;
        StoreUntilDate = storeUntilDate;
        DeliveryCodeHash = deliveryCodeHash;
        Order = order;
        Postomat = postomat;
        CreatedBy = createdBy;
        DeliveredBy = deliveredBy;
        DeliveredBackBy = deliveredBackBy;
    }

    public Guid Id { get; }
    public string Status { get; }
    public DateTime LastStatusChangeDate { get; }
    public DateTime? StoreUntilDate { get; }
    public string DeliveryCodeHash { get; }
    public Order Order { get; }
    public Postomat Postomat { get; }
    public User CreatedBy { get; }
    public User? DeliveredBy { get; }
    public User? DeliveredBackBy { get; }

    private static string BasicChecks(string status, DateTime lastStatusChangeDate, string deliveryCodeHash)
    {
        var error = string.Empty;

        if (status.Length > MaxStatusLength)
        {
            error = $"Status can't be longer than {MaxStatusLength} characters or empty.";
        }
        else if (!AllowedStatuses.Contains(status))
        {
            error = $"Unknown status \"{status}\". Use one of [{string.Join(", ", AllowedStatuses)}].";
        }
        else if (deliveryCodeHash.Length > MaxDeliveryCodeHashLength)
        {
            error = $"Delivery code hash can't be longer than {MaxDeliveryCodeHashLength} characters or empty.";
        }
        else if (lastStatusChangeDate > DateTime.Now)
        {
            error = "Last status change date can't be from the future.";
        }

        return error;
    }

    public static (OrderPlan OrderPlan, string Error) Create(Guid id, string status, DateTime lastStatusChangeDate,
        DateTime? storeUntilDate, string deliveryCodeHash, Order order, Postomat postomat, User createdBy,
        User? deliveredBy, User? deliveredBackBy)
    {
        var error = BasicChecks(status, lastStatusChangeDate, deliveryCodeHash);

        var orderPlan = new OrderPlan(id, status, lastStatusChangeDate, storeUntilDate, deliveryCodeHash, order,
            postomat, createdBy, deliveredBy, deliveredBackBy);

        return (orderPlan, error);
    }

    public static string DeliveryCodeCheck(string deliveryCode)
    {
        var deliveryCodeCheckError = string.Empty;

        if (deliveryCode.Length < 8)
        {
            deliveryCodeCheckError = "The delivery code must be longer than 8 characters.";
        }
        else if (!deliveryCode.All(char.IsLetterOrDigit))
        {
            deliveryCodeCheckError = "The delivery code must consist only of letters and numbers.";
        }

        return deliveryCodeCheckError;
    }
}