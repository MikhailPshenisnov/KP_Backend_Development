using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
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
        try
        {
            var user = (await _usersService.GetFilteredUsersAsync(null, ct))
                .FirstOrDefault(u => u.Login == login);

            if (user == null)
                throw new Exception("Invalid login or password");

            if (login == user.Login &&
                BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash))
            {
                var token = GenerateJwtToken(user.Id, user.Role.Id);

                return token;
            }

            throw new Exception("Invalid login or password");
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to login user: {e.Message}");
        }
    }

    public async Task<(Guid userId, Guid roleId)> ValidateToken(string token, CancellationToken ct)
    {
        try
        {
            var principal = ValidateJwtToken(token);

            var uId = Guid.Parse(principal.FindFirst("userId")?.Value!);
            var rId = Guid.Parse(principal.FindFirst("roleId")?.Value!);

            return (uId, rId);
        }
        catch (Exception e)
        {
            throw new Exception($"Invalid token: {e.Message}");
        }
    }

    private string GenerateJwtToken(Guid userId, Guid roleId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("very_strong_and_super_super_secret_key_123!"); /* TODO */
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
        return tokenHandler.WriteToken(token);
    }

    private ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("very_strong_and_super_super_secret_key_123!"); /* TODO */
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
        return principal;
    }
}