using Postomat.Core.Models;
using Postomat.Core.Models.Other;

namespace Postomat.DataAccess.Database.Entities;

public partial class Order
{
    public Guid Id { get; set; }
    public string ReceivingCodeHash { get; set; } = null!;
    public SizeEnumerator OrderSize { get; set; }

    public virtual ICollection<Cell> Cells { get; set; } = new List<Cell>();
    public virtual ICollection<OrderPlan> OrderPlans { get; set; } = new List<OrderPlan>();
}