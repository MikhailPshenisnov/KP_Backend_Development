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
[Route("[controller]")]
public class AdminController : ControllerBase
{
    private readonly IOrderPlansService _orderPlansService;
    private readonly IOrdersService _ordersService;
    private readonly IPostomatsService _postomatsService;
    private readonly IRolesService _rolesService;
    private readonly IUsersService _usersService;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;
    private readonly IRequestClient<MicroserviceCreateLogRequest> _createLogClient;
    private readonly IRequestClient<MicroserviceGetFilteredLogsRequest> _getFilteredLogsClient;
    private readonly IRequestClient<MicroserviceUpdateLogRequest> _updateLogClient;
    private readonly IRequestClient<MicroserviceDeleteLogRequest> _deleteLogClient;
    private readonly IControllerErrorLogService _controllerErrorLogService;

    public AdminController(IOrderPlansService orderPlansService, IOrdersService ordersService,
        IPostomatsService postomatsService, IRolesService rolesService, IUsersService usersService,
        IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient,
        IRequestClient<MicroserviceCreateLogRequest> createLogClient,
        IRequestClient<MicroserviceGetFilteredLogsRequest> getFilteredLogsClient,
        IRequestClient<MicroserviceUpdateLogRequest> updateLogClient,
        IRequestClient<MicroserviceDeleteLogRequest> deleteLogClient,
        IControllerErrorLogService controllerErrorLogService)
    {
        _orderPlansService = orderPlansService;
        _ordersService = ordersService;
        _postomatsService = postomatsService;
        _rolesService = rolesService;
        _usersService = usersService;
        _validateTokenClient = validateTokenClient;
        _createLogClient = createLogClient;
        _getFilteredLogsClient = getFilteredLogsClient;
        _updateLogClient = updateLogClient;
        _deleteLogClient = deleteLogClient;
        _controllerErrorLogService = controllerErrorLogService;
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

    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> ReceiveOrder([FromQuery] ReceiveOrderRequest receiveOrderRequest,
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

            return Redirect("/Customer/ReceiveOrder");
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<ReceiveOrderResponse>(
                "Admin controller", "Error while receiving order", e.Message));
        }
    }

    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> DeliverOrder([FromQuery] DeliverOrderRequest deliverOrderRequest,
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

            return Redirect("/Delivery/DeliverOrder");
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<DeliverOrderResponse>(
                "Admin controller", "Error while delivering order", e.Message));
        }
    }

    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> DeliverOrderBack([FromQuery] DeliverOrderBackRequest deliverOrderBackRequest,
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

            return Redirect("/Delivery/DeliverOrderBack");
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<DeliverOrderBackResponse>(
                "Admin controller", "Error while delivering order back", e.Message));
        }
    }
}