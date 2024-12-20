namespace Postomat.DataAccess.Database.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Login { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Guid RoleId { get; set; }

    public virtual ICollection<OrderPlan> OrderPlanCreatedByNavigations { get; set; } = new List<OrderPlan>();
    public virtual ICollection<OrderPlan> OrderPlanDeliveredBackByNavigations { get; set; } = new List<OrderPlan>();
    public virtual ICollection<OrderPlan> OrderPlanDeliveredByNavigations { get; set; } = new List<OrderPlan>();
    public virtual Role Role { get; set; } = null!;
}