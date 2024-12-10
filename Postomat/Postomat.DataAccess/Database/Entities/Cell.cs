namespace Postomat.DataAccess.Database.Entities;

public partial class Cell
{
    public Guid Id { get; set; }
    public int CellSize { get; set; }
    public Guid PostomatId { get; set; }
    public Guid? OrderId { get; set; }

    public virtual Order? Order { get; set; }
    public virtual Postomat Postomat { get; set; } = null!;
}