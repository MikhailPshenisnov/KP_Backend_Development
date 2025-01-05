namespace Postomat.Core.Contracrs;

public record BaseResponse<T>(
    T? Data,
    string? ErrorMessage
);