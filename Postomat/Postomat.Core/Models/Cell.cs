namespace Postomat.Core.Models;

public class Cell
{
    private Cell(Guid id, SizeEnumerator cellSize, Guid postomatId, Order? order)
    {
        Id = id;
        CellSize = cellSize;
        PostomatId = postomatId;
        Order = order;
    }

    public Guid Id { get; }
    public SizeEnumerator CellSize { get; }
    public Guid PostomatId { get; }
    public Order? Order { get; }

    private static string BasicChecks(SizeEnumerator cellSize, Order? order)
    {
        var error = string.Empty;

        if (order is not null && cellSize < order.OrderSize)
        {
            error = $"Incorrect order size: {order.OrderSize} > {cellSize}";
        }

        return error;
    }

    public static (Cell Cell, string Error) Create(Guid id, SizeEnumerator cellSize, Guid postomatId, Order? order)
    {
        var error = BasicChecks(cellSize, order);

        var cell = new Cell(id, cellSize, postomatId, order);

        return (cell, error);
    }
}