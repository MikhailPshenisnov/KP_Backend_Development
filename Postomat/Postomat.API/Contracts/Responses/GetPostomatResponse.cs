namespace Postomat.API.Contracts.Responses;

public record GetPostomatResponse(
    Guid Id,
    string Name,
    string Address
);