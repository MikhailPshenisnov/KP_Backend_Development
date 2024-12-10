namespace Postomat.DataAccess.Database.Entities;

public partial class Role
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = null!;
    public int AccessLvl { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}