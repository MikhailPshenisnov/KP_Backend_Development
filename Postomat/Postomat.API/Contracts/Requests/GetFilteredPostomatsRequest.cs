namespace Postomat.API.Contracts.Requests;

public record GetFilteredPostomatsRequest(
    string? PartOfName,
    string? PartOfAddress
);