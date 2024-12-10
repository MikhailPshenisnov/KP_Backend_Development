namespace Postomat.DataAccess.Database.Entities;

public partial class OrderPlan
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
    public DateTime LastStatusChangeDate { get; set; }
    public DateTime? StoreUntilDate { get; set; }
    public string DeliveryCodeHash { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Guid PostomatId { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? DeliveredBy { get; set; }
    public Guid? DeliveredBackBy { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual User? DeliveredBackByNavigation { get; set; }
    public virtual User? DeliveredByNavigation { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual Postomat Postomat { get; set; } = null!;
}