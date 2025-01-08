namespace Postomat.API.Contracts.Requests;

public record CreatePostomatRequest(
    string Name,
    string Address
);