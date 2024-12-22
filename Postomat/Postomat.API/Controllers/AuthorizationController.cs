using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
    private readonly IRequestClient<MicroserviceLoginUserRequest> _loginUserClient;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;

    public AuthorizationController(IRequestClient<MicroserviceLoginUserRequest> loginUserClient,
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
                new MicroserviceLoginUserRequest(
                    loginUserRequest.Login,
                    loginUserRequest.Password
                ));

            if (!response.Message.IsSuccess)
                return Unauthorized(new BaseResponse<LoginUserResponse>(
                    null,
                    response.Message.ErrorMessage ?? ""
                ));

            if (response.Message.Token is null)
                throw new Exception("If login is successful, the token cannot be empty.");

            Response.Cookies.Append("jwt_token", response.Message.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new BaseResponse<LoginUserResponse>(
                new LoginUserResponse("Successful", response.Message.Token),
                null
            ));
        }
        catch (Exception e)
        {
            /* TODO */ // логирование
            return Ok(new BaseResponse<LoginUserResponse>(
                null,
                e.Message
            ));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUserToken()
    {
        try
        {
            Request.Cookies.TryGetValue("jwt_token", out var jwtToken);

            return Ok(new BaseResponse<GetCurrentUserTokenResponse>(
                new GetCurrentUserTokenResponse(jwtToken ?? ""),
                null
            ));
        }
        catch (Exception e)
        {
            /* TODO */
            return Ok(new BaseResponse<GetCurrentUserTokenResponse>(
                null,
                e.Message
            ));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest validateTokenRequest)
    {
        try
        {
            var response = await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
                new MicroserviceValidateTokenRequest(validateTokenRequest.Token)
            );

            if (!response.Message.IsValid)
                return Unauthorized(new BaseResponse<ValidateTokenResponse>(
                    null,
                    response.Message.ErrorMessage ?? ""
                ));

            if (response.Message.User is null)
            {
                throw new Exception("If token is valid, the user cannot be empty.");
            }

            return Ok(new BaseResponse<ValidateTokenResponse>(
                new ValidateTokenResponse(
                    "Token is valid",
                    response.Message.User.UserId,
                    response.Message.User.RoleId),
                null
            ));
        }
        catch (Exception e)
        {
            /* TODO */
            return Ok(new BaseResponse<ValidateTokenResponse>(
                null,
                e.Message
            ));
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> LogoutUser()
    {
        try
        {
            Response.Cookies.Append("jwt_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new BaseResponse<LogoutUserResponse>
            (
                new LogoutUserResponse("Successful"),
                null
            ));
        }
        catch (Exception e)
        {
            /* TODO */
            return Ok(new BaseResponse<LogoutUserResponse>
            (
                null,
                e.Message
            ));
        }
    }
}