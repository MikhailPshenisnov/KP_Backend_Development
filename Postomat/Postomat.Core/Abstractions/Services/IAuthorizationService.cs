namespace Postomat.Core.Abstractions.Services;

public interface IAuthorizationService
{
    Task<string> LoginUser(string login, string password, CancellationToken ct);
    Task<(Guid userId, Guid roleId)> ValidateToken(string token, CancellationToken ct);
}