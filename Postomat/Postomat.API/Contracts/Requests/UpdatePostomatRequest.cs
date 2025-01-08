namespace Postomat.API.Contracts.Requests;

public record UpdatePostomatRequest(
    Guid PostomatId,
    string NewPostomatName,
    string NewPostomatAddress
);