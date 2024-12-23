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
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly IUsersService _usersService;
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;
    private readonly IControllerErrorLogService _controllerErrorLogService;

    public DeliveryController(IDeliveryService deliveryService, IUsersService usersService,
        IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient,
        IControllerErrorLogService controllerErrorLogService)
    {
        _deliveryService = deliveryService;
        _usersService = usersService;
        _validateTokenClient = validateTokenClient;
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
    [HttpGet]
    public async Task<IActionResult> DeliverOrder([FromQuery] DeliverOrderRequest deliverOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            var authToken = authHeader.ToString().Replace("Bearer ", "");

            var (checkResult, user) = await CheckAccessLvl(
                authToken, (int)AccessLvlEnumerator.FiredEmployee - 1, cancellationToken);
            if (!checkResult)
                throw new Exception("The user does not have sufficient access rights");

            await _deliveryService.DeliverOrderAsync(user!, deliverOrderRequest.DeliveryCode,
                deliverOrderRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<DeliverOrderResponse>(
                new DeliverOrderResponse("Order delivered back successfully"), null));
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<DeliverOrderResponse>(
                "Delivery controller", "Error while delivering order", e.Message));
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

            var (checkResult, user) = await CheckAccessLvl(
                authToken, (int)AccessLvlEnumerator.FiredEmployee - 1, cancellationToken);
            if (!checkResult)
                throw new Exception("The user does not have sufficient access rights");

            await _deliveryService.DeliverOrderBackAsync(user!, deliverOrderBackRequest.DeliveryCode,
                deliverOrderBackRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<DeliverOrderBackResponse>(
                new DeliverOrderBackResponse("Order delivered successfully"), null));
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<DeliverOrderBackResponse>(
                "Delivery controller", "Error while delivering order back", e.Message));
        }
    }
}