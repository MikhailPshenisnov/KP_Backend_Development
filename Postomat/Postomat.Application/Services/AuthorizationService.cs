using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Postomat.Application.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUsersService _usersService;
    private readonly IConfiguration _configuration;

    public AuthorizationService(IUsersService usersService, IConfiguration configuration)
    {
        _usersService = usersService;
        _configuration = configuration;
    }

    public async Task<string> LoginUser(string login, string password, CancellationToken ct)
    {
        try
        {
            var user = (await _usersService.GetFilteredUsersAsync(null, ct))
                .FirstOrDefault(u => u.Login == login);

            if (user == null)
                throw new ServiceException("Invalid login or password.");

            if (login != user.Login || !BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash))
                throw new ServiceException("Invalid login or password.");

            var token = await GenerateJwtToken(user.Id, user.Role.Id);
            return token;
        }
        catch (Exception e)
        {
            throw new ServiceException($"Unable to login user. " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<(Guid userId, Guid roleId)> ValidateToken(string token, CancellationToken ct)
    {
        try
        {
            var principal = await ValidateJwtToken(token);

            var uId = Guid.Parse(principal.FindFirst("userId")?.Value!);
            var rId = Guid.Parse(principal.FindFirst("roleId")?.Value!);

            return (uId, rId);
        }
        catch (Exception e)
        {
            throw new ServiceException($"Invalid token. " +
                                       $"--> {e.Message}");
        }
    }

    private Task<string> GenerateJwtToken(Guid userId, Guid roleId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var authorizationConfig = _configuration.GetSection("AuthorizationServiceKeys");
        var key = Encoding.UTF8.GetBytes(authorizationConfig["ValidationSecretKey"] ?? string.Empty);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("userId", userId.ToString()),
                new Claim("roleId", roleId.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }

    private Task<ClaimsPrincipal> ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var authorizationConfig = _configuration.GetSection("AuthorizationServiceKeys");
        var key = Encoding.UTF8.GetBytes(authorizationConfig["ValidationSecretKey"] ?? string.Empty);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
        return Task.FromResult(principal);
    }
}