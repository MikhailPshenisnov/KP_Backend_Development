using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Postomat.AuthorizationMicroservice.Contracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;
using Postomat.Core.Models.Other;

namespace Postomat.AuthorizationMicroservice.Consumers;

public class LoginUserConsumer : IConsumer<MicroserviceLoginUserRequest>
{
    public async Task Consume(ConsumeContext<MicroserviceLoginUserRequest> context)
    {
        var request = context.Message;

        var user = User.Create(
            Guid.NewGuid(),
            "admin",
            BCrypt.Net.BCrypt.EnhancedHashPassword("admin"),
            Role.Create(Guid.NewGuid(), "administrator", (int)AccessLvlEnumerator.Administrator).Role).User;

        /* TODO */
        if (request.Login == user.Login &&
            (BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash)))
        {
            var token = GenerateJwtToken(new TokenDto(
                user.Id,
                user.Role.Id
            ));

            await context.RespondAsync(new MicroserviceLoginUserResponse(
                true,
                token,
                null));
        }

        await context.RespondAsync(new MicroserviceLoginUserResponse(
            false,
            null,
            "Invalid login or password"));
    }

    private string GenerateJwtToken(TokenDto tokenDto)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("very_strong_and_super_super_secret_key_123!"); /* TODO */
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("userid", tokenDto.UserId.ToString()),
                new Claim("roleid", tokenDto.RoleId.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}