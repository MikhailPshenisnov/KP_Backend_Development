using Postomat.Core.Models.Other;

namespace Postomat.API.Contracts.Responses;

public record AddCellToPostomatResponse(
    Guid AddedCellId
);