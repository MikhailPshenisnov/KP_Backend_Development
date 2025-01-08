namespace Postomat.API.Contracts.Requests;

public record DeleteCellFromPostomatRequest(
    Guid CellId
);