using Postomat.Core.Models;

namespace Postomat.Core.Abstractions;

public interface ICellsRepository
{
    Task<Guid> CreateCell(Cell cell);
    Task<List<Cell>> GetAllCells();
    Task<Guid> UpdateCell(Guid cellId, Cell newCell);
    Task<Guid> DeleteCell(Guid cellId);
}