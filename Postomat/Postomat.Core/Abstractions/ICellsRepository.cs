using Postomat.Core.Models;

namespace Postomat.Core.Abstractions;

public interface ICellsRepository
{
    Task<Guid> CreateCell(Cell cell);
    Task<List<Cell>> GetPostomatCells(Guid postomatId);
    Task<Guid> UpdateCell(Guid cellId, Cell newCell);
    Task<Guid> DeleteCell(Guid cellId);
}