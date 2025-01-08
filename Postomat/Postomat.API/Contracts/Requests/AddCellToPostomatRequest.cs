using Postomat.Core.Models.Other;

namespace Postomat.API.Contracts.Requests;

public record AddCellToPostomatRequest(
    Guid PostomatId,
    SizeEnumerator CellSize
);