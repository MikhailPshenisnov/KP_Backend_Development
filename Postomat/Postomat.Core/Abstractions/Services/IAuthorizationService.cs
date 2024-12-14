namespace Postomat.Core.Abstractions.Services;

public interface IAuthorizationService
{
    Task<Guid> RegisterUserAsync(string login, string password, CancellationToken ct);
    Task<string> LoginUserAsync(string login, string password, CancellationToken ct);
    Task<string> LogoutUserAsync(CancellationToken ct);
    Task<int> GetAccessLvlFromTokenAsync(string login, string password, CancellationToken ct);
}