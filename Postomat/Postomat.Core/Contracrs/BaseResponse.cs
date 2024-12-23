namespace Postomat.Core.Contracrs;

public record BaseResponse<T>(
    object? Data,
    string? ErrorMessage
);