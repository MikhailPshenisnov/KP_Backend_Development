using Postomat.Core.Models.Other;

namespace Postomat.DataAccess.Database.Entities;

public class Cell
{
    public Guid Id { get; set; }
    public SizeEnumerator CellSize { get; set; }
    public Guid PostomatId { get; set; }
    public Guid? OrderId { get; set; }

    public virtual Order? Order { get; set; }
    public virtual Postomat Postomat { get; set; } = null!;
}