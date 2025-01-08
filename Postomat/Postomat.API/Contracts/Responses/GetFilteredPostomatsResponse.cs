namespace Postomat.API.Contracts.Responses;

public record GetFilteredPostomatsResponse(
    Guid Id,
    string Name,
    string Address
);