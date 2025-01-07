using ConsumerException = Postomat.Core.Exceptions.BaseExceptions.ConsumerException;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models.Other;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
    private readonly IRequestClient<MicroserviceLoginUserRequest> _loginUserClient;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;
    private readonly IAccessCheckService _accessCheckService;

    public AuthorizationController(IRequestClient<MicroserviceLoginUserRequest> loginUserClient,
        IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient,
        IAccessCheckService accessCheckService)
    {
        _loginUserClient = loginUserClient;
        _validateTokenClient = validateTokenClient;
        _accessCheckService = accessCheckService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserRequest loginUserRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = (await _loginUserClient.GetResponse<MicroserviceLoginUserResponse>(
                    new MicroserviceLoginUserRequest(loginUserRequest.Login, loginUserRequest.Password),
                    cancellationToken))
                .Message;

            if (response.ErrorMessage is not null)
            {
                throw new ConsumerException($"Something went wrong while login process. " +
                                            $"--> {response.ErrorMessage}");
            }

            if (!response.IsSuccess)
            {
                if (response.Message is null)
                    throw new ConsumerException("If validation is not success, the message cannot be empty.");
                return Unauthorized(new BaseResponse<LoginUserResponse>(
                    null,
                    response.Message));
            }

            if (response.Token is null)
                throw new ConsumerException("If login is successful, the token cannot be empty.");

            Response.Cookies.Append("jwt_token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new BaseResponse<LoginUserResponse>(
                new LoginUserResponse(response.Token),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while login process. " +
                                          $"--> {e.Message}");
        }
    }

    [HttpGet]
    public Task<IActionResult> GetCurrentUserToken(CancellationToken cancellationToken)
    {
        try
        {
            Request.Cookies.TryGetValue("jwt_token", out var jwtToken);

            return Task.FromResult<IActionResult>(Ok(new BaseResponse<GetCurrentUserTokenResponse>(
                new GetCurrentUserTokenResponse(jwtToken ?? string.Empty),
                null)));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while getting current user token. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest validateTokenRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var response = (await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
                new MicroserviceValidateTokenRequest(validateTokenRequest.Token), cancellationToken)).Message;
            if (response.ErrorMessage is not null)
                throw new ConsumerException($"Something went wrong while token validation process. " +
                                            $"--> {response.ErrorMessage}");
            if (!response.IsValid)
            {
                if (response.Message is null)
                    throw new ConsumerException("If validation is not success, the message cannot be empty.");
                return Ok(new BaseResponse<ValidateTokenResponse>(
                    null,
                    response.Message));
            }

            if (response.UserDto is null)
                throw new ConsumerException("If token is valid, the user cannot be empty.");

            return Ok(new BaseResponse<ValidateTokenResponse>(
                new ValidateTokenResponse(
                    response.UserDto.UserId,
                    response.UserDto.RoleId),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while validating token. " +
                                          $"--> {e.Message}");
        }
    }

    [HttpGet]
    public Task<IActionResult> LogoutUser()
    {
        try
        {
            Response.Cookies.Append("jwt_token", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Task.FromResult<IActionResult>(Ok(new BaseResponse<LogoutUserResponse>(
                new LogoutUserResponse("Successful."),
                null)));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while logout user. " +
                                          $"--> {e.Message}");
        }
    }
}