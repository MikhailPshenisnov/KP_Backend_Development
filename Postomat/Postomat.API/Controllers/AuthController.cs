using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IRequestClient<MicroserviceLoginUserRequest> _loginUserClient;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;

    public AuthController(IRequestClient<MicroserviceLoginUserRequest> loginUserClient,
        IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient)
    {
        _loginUserClient = loginUserClient;
        _validateTokenClient = validateTokenClient;
    }

    [HttpPost]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserRequest loginUserRequest)
    {
        try
        {
            var response = await _loginUserClient.GetResponse<MicroserviceLoginUserResponse>(
                new MicroserviceLoginUserRequest
                {
                    Login = loginUserRequest.Login,
                    Password = loginUserRequest.Password
                });

            if (response.Message.IsSuccess)
            {
                Response.Cookies.Append("jwt_token", response.Message.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                return Ok(new BaseResponse<LoginUserResponse>
                {
                    Data = new LoginUserResponse { Message = "Login successful" },
                    ErrorMessage = null
                });
            }

            return Unauthorized(new BaseResponse<LoginUserResponse>
            {
                Data = null,
                ErrorMessage = response.Message.Message
            });
        }
        catch (Exception e)
        {
            /* TODO */
            // логирование
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest validateTokenRequest)
    {
        try
        {
            var response = await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
                new MicroserviceValidateTokenRequest
                {
                    Token = validateTokenRequest.Token
                });

            if (response.Message.IsValid)
            {
                return Ok(new BaseResponse<ValidateTokenResponse>
                {
                    Data = new ValidateTokenResponse
                    {
                        Message = "Token is valid",
                        UserId = response.Message.UserId,
                        RoleId = response.Message.RoleId
                    },
                    ErrorMessage = null
                });
            }

            return Unauthorized(new BaseResponse<ValidateTokenResponse>
            {
                Data = null,
                ErrorMessage = response.Message.Message
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }
}