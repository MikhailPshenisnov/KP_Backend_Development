using Postomat.Core.Abstractions.Services;

namespace Postomat.Application.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUsersService _usersService;

    public AuthorizationService(IUsersService usersService)
    {
        _usersService = usersService;
    }


    public async Task<string> LoginUser(string login, string password, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<(Guid userId, Guid roleId)> ValidateToken(string token, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}