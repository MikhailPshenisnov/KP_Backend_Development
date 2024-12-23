using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;
using Postomat.Core.Models.Other;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly IUsersService _usersService;
    private readonly IRolesService _rolesService;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;
    private readonly IRequestClient<MicroserviceCreateLogRequest> _createLogClient;

    public DeliveryController(IDeliveryService deliveryService, IUsersService usersService, IRolesService rolesService,
        IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient,
        IRequestClient<MicroserviceCreateLogRequest> createLogClient)
    {
        _deliveryService = deliveryService;
        _usersService = usersService;
        _rolesService = rolesService;
        _validateTokenClient = validateTokenClient;
        _createLogClient = createLogClient;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> DeliverOrder([FromQuery] DeliverOrderRequest deliverOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            var authToken = authHeader.ToString().Replace("Bearer ", "");

            var response = await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
                new MicroserviceValidateTokenRequest(authToken), cancellationToken);

            if (response.Message.IsValid)
            {
                var role = await _rolesService.GetRoleAsync(response.Message.User!.RoleId, cancellationToken);
                if (role.AccessLvl >= (int)AccessLvlEnumerator.FiredEmployee)
                    throw new Exception("The user does not have sufficient access rights");
            }
            else
            {
                return Unauthorized(new BaseResponse<DeliverOrderResponse>(
                    new DeliverOrderResponse("Invalid token"),
                    null
                ));
            }

            var user = await _usersService.GetUserAsync(response.Message.User.UserId, cancellationToken);

            await _deliveryService.DeliverOrderBackAsync(user, deliverOrderRequest.DeliveryCode,
                deliverOrderRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<DeliverOrderResponse>
            (
                new DeliverOrderResponse("Order delivered back successfully"),
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
                    "Delivery controller",
                    "Error",
                    "Error while delivering order",
                    e.Message);
                if (error is not null)
                    throw new Exception($"Unable to create error log: {error}");

                var response = await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                    new MicroserviceCreateLogRequest(log)
                );
                if (response.Message.ErrorMessage is not null)
                    throw new Exception($"Unable to create error log (microservice error): {error}");

                return Ok(new BaseResponse<DeliverOrderResponse>(
                    null,
                    e.Message + $" Error log was created: \"{response.Message.CreatedLogId}\""
                ));
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponse<DeliverOrderResponse>(
                    null,
                    e.Message + $" Error log was not created: \"{ex.Message}\""
                ));
            }
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> DeliverOrderBack([FromQuery] DeliverOrderBackRequest deliverOrderBackRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            var authToken = authHeader.ToString().Replace("Bearer ", "");

            var response = await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
                new MicroserviceValidateTokenRequest(authToken), cancellationToken);

            if (response.Message.IsValid)
            {
                var role = await _rolesService.GetRoleAsync(response.Message.User!.RoleId, cancellationToken);
                if (role.AccessLvl >= (int)AccessLvlEnumerator.FiredEmployee)
                    throw new Exception("The user does not have sufficient access rights");
            }
            else
            {
                return Unauthorized(new BaseResponse<DeliverOrderBackResponse>(
                    new DeliverOrderBackResponse("Invalid token"),
                    null
                ));
            }

            var user = await _usersService.GetUserAsync(response.Message.User.UserId, cancellationToken);

            await _deliveryService.DeliverOrderBackAsync(user, deliverOrderBackRequest.DeliveryCode,
                deliverOrderBackRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<DeliverOrderBackResponse>
            (
                new DeliverOrderBackResponse("Order delivered successfully"),
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
                    "Delivery controller",
                    "Error",
                    "Error while delivering order back",
                    e.Message);
                if (error is not null)
                    throw new Exception($"Unable to create error log: {error}");

                var response = await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                    new MicroserviceCreateLogRequest(log)
                );
                if (response.Message.ErrorMessage is not null)
                    throw new Exception($"Unable to create error log (microservice error): {error}");

                return Ok(new BaseResponse<DeliverOrderBackResponse>(
                    null,
                    e.Message + $" Error log was created: \"{response.Message.CreatedLogId}\""
                ));
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponse<DeliverOrderBackResponse>(
                    null,
                    e.Message + $" Error log was not created: \"{ex.Message}\""
                ));
            }
        }
    }
}