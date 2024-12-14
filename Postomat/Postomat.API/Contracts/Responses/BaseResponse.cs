namespace Postomat.API.Contracts.Responses;

public record BaseResponse<T>(
    T? Data,
    string? ErrorMessage
);