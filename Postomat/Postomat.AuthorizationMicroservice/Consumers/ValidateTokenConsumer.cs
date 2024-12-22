using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.AuthorizationMicroservice.Consumers;

public class ValidateTokenConsumer : IConsumer<MicroserviceValidateTokenRequest>
{
    public async Task Consume(ConsumeContext<MicroserviceValidateTokenRequest> context)
    {
        var request = context.Message;

        try
        {
            var principal = ValidateJwtToken(request.Token);

            await context.RespondAsync(new MicroserviceValidateTokenResponse(
                true,
                new UserDto(
                    Guid.Parse(principal.FindFirst("userid")?.Value!),
                    Guid.Parse(principal.FindFirst("roleid")?.Value!)),
                null));
        }
        catch (Exception)
        {
            await context.RespondAsync(new MicroserviceValidateTokenResponse(
                false,
                null,
                "Invalid token"));
        }
    }

    private ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var k = "1231231231231";
        var ky = Encoding.Unicode.GetBytes(k);
        var key = "very_strong_and_super_super_secret_key_123!"u8.ToArray(); /* TODO */
        var kkey = Encoding.ASCII.GetBytes("super_secret_key_123!");
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