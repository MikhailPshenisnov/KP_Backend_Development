namespace Postomat.DataAccess.Database.Entities;

public partial class Postomat
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;

    public virtual ICollection<Cell> Cells { get; set; } = new List<Cell>();
    public virtual ICollection<OrderPlan> OrderPlans { get; set; } = new List<OrderPlan>();
}