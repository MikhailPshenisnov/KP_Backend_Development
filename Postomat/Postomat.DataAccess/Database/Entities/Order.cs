namespace Postomat.DataAccess.Database.Entities;

public partial class Order
{
    public Guid Id { get; set; }
    public string ReceivingCodeHash { get; set; } = null!;
    public int OrderSize { get; set; }

    public virtual ICollection<Cell> Cells { get; set; } = new List<Cell>();
    public virtual ICollection<OrderPlan> OrderPlans { get; set; } = new List<OrderPlan>();
}