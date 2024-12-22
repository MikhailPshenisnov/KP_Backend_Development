using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
    private readonly IRequestClient<MicroserviceLoginUserRequest> _loginUserClient;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;
    private readonly IRequestClient<MicroserviceCreateLogRequest> _createLogClient;

    public AuthorizationController(IRequestClient<MicroserviceLoginUserRequest> loginUserClient,
        IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient,
        IRequestClient<MicroserviceCreateLogRequest> createLogClient)
    {
        _loginUserClient = loginUserClient;
        _validateTokenClient = validateTokenClient;
        _createLogClient = createLogClient;
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
            try
            {
                var (log, error) = Log.Create(
                    Guid.NewGuid(),
                    DateTime.Now.ToUniversalTime(),
                    "Authorization controller",
                    "Error",
                    "Error while login process",
                    e.Message);
                if (error is not null)
                    throw new Exception($"Unable to create error log: {error}");
                
                var response = await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                    new MicroserviceCreateLogRequest(log)
                );
                if (response.Message.ErrorMessage is not null)
                    throw new Exception($"Unable to create error log (microservice error): {error}");
                
                return Ok(new BaseResponse<LoginUserResponse>(
                    null,
                    e.Message + $" Error log was created: \"{response.Message.CreatedLogId}\""
                ));
                
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponse<LoginUserResponse>(
                    null,
                    e.Message + $" Error log was not created: \"{ex.Message}\""
                ));
            }
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
            try
            {
                var (log, error) = Log.Create(
                    Guid.NewGuid(),
                    DateTime.Now.ToUniversalTime(),
                    "Authorization controller",
                    "Error",
                    "Error while getting current user token",
                    e.Message);
                if (error is not null)
                    throw new Exception($"Unable to create error log: {error}");
                
                var response = await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                    new MicroserviceCreateLogRequest(log)
                );
                if (response.Message.ErrorMessage is not null)
                    throw new Exception($"Unable to create error log (microservice error): {error}");
                
                return Ok(new BaseResponse<GetCurrentUserTokenResponse>(
                    null,
                    e.Message + $" Error log was created: \"{response.Message.CreatedLogId}\""
                ));
                
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponse<GetCurrentUserTokenResponse>(
                    null,
                    e.Message + $" Error log was not created: \"{ex.Message}\""
                ));
            }
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
            try
            {
                var (log, error) = Log.Create(
                    Guid.NewGuid(),
                    DateTime.Now.ToUniversalTime(),
                    "Authorization controller",
                    "Error",
                    "Error while validating token",
                    e.Message);
                if (error is not null)
                    throw new Exception($"Unable to create error log: {error}");
                
                var response = await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                    new MicroserviceCreateLogRequest(log)
                );
                if (response.Message.ErrorMessage is not null)
                    throw new Exception($"Unable to create error log (microservice error): {error}");
                
                return Ok(new BaseResponse<ValidateTokenResponse>(
                    null,
                    e.Message + $" Error log was created: \"{response.Message.CreatedLogId}\""
                ));
                
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponse<ValidateTokenResponse>(
                    null,
                    e.Message + $" Error log was not created: \"{ex.Message}\""
                ));
            }
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
            try
            {
                var (log, error) = Log.Create(
                    Guid.NewGuid(),
                    DateTime.Now.ToUniversalTime(),
                    "Authorization controller",
                    "Error",
                    "Error while logout user",
                    e.Message);
                if (error is not null)
                    throw new Exception($"Unable to create error log: {error}");
                
                var response = await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                    new MicroserviceCreateLogRequest(log)
                );
                if (response.Message.ErrorMessage is not null)
                    throw new Exception($"Unable to create error log (microservice error): {error}");
                
                return Ok(new BaseResponse<LogoutUserResponse>(
                    null,
                    e.Message + $" Error log was created: \"{response.Message.CreatedLogId}\""
                ));
                
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponse<LogoutUserResponse>(
                    null,
                    e.Message + $" Error log was not created: \"{ex.Message}\""
                ));
            }
        }
    }
}