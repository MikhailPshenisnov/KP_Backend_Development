using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;
using Postomat.Core.Models.Other;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
    private readonly IRequestClient<MicroserviceLoginUserRequest> _loginUserClient;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;
    private readonly IControllerErrorLogService _controllerErrorLogService;
    private readonly IUsersService _usersService;

    public AuthorizationController(IRequestClient<MicroserviceLoginUserRequest> loginUserClient,
        IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient,
        IControllerErrorLogService controllerErrorLogService,
        IUsersService usersService)
    {
        _loginUserClient = loginUserClient;
        _validateTokenClient = validateTokenClient;
        _controllerErrorLogService = controllerErrorLogService;
        _usersService = usersService;
    }

    private async Task<(bool CheckResult, User? User)> CheckAccessLvl(string token, int minAccessLvl,
        CancellationToken cancellationToken)
    {
        var response = (await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
            new MicroserviceValidateTokenRequest(token), cancellationToken)).Message;
        if (!response.IsValid)
            return (false, null);
        if (response.UserDto is null)
            throw new Exception("If token is valid, the user cannot be empty");

        var user = await _usersService.GetUserAsync(response.UserDto.UserId, cancellationToken);

        return (user.Role.AccessLvl <= minAccessLvl, user);
    }

    [HttpPost]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserRequest loginUserRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _loginUserClient.GetResponse<MicroserviceLoginUserResponse>(
                new MicroserviceLoginUserRequest(
                    loginUserRequest.Login,
                    loginUserRequest.Password),
                cancellationToken);

            if (!response.Message.IsSuccess)
                return Unauthorized(new BaseResponse<LoginUserResponse>(
                    null,
                    response.Message.ErrorMessage ?? string.Empty));

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
                null));
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<LoginUserResponse>(
                "Authorization controller", "Error while login process", e.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUserToken(CancellationToken cancellationToken)
    {
        try
        {
            Request.Cookies.TryGetValue("jwt_token", out var jwtToken);

            return Ok(new BaseResponse<GetCurrentUserTokenResponse>(
                new GetCurrentUserTokenResponse(jwtToken ?? string.Empty),
                null));
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<GetCurrentUserTokenResponse>(
                "Authorization controller", "Error while getting current user token", e.Message));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest validateTokenRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            var authToken = authHeader.ToString().Replace("Bearer ", string.Empty);

            var (checkResult, user) = await CheckAccessLvl(
                authToken, (int)AccessLvlEnumerator.DeliveryMan - 1, cancellationToken);
            if (!checkResult)
                throw new Exception("The user does not have sufficient access rights");

            var response = (await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
                new MicroserviceValidateTokenRequest(validateTokenRequest.Token), cancellationToken)).Message;
            if (!response.IsValid)
                return Ok(new BaseResponse<ValidateTokenResponse>(null, response.ErrorMessage ?? string.Empty));
            if (response.UserDto is null)
                throw new Exception("If token is valid, the user cannot be empty.");

            return Ok(new BaseResponse<ValidateTokenResponse>(
                new ValidateTokenResponse(
                    "Token is valid",
                    response.UserDto.UserId,
                    response.UserDto.RoleId),
                null));
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<ValidateTokenResponse>(
                "Authorization controller", "Error while validating token", e.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LogoutUser()
    {
        try
        {
            Response.Cookies.Append("jwt_token", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new BaseResponse<LogoutUserResponse>(
                new LogoutUserResponse("Successful"),
                null));
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<LogoutUserResponse>(
                "Authorization controller", "Error while logout user", e.Message));
        }
    }
}