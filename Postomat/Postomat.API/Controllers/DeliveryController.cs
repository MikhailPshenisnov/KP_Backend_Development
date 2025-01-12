using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Models.Other;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly IAccessCheckService _accessCheckService;

    public DeliveryController(IDeliveryService deliveryService, IAccessCheckService accessCheckService)
    {
        _deliveryService = deliveryService;
        _accessCheckService = accessCheckService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> DeliverOrder([FromQuery] DeliverOrderRequest deliverOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.FiredEmployee - 1,
                cancellationToken);

            await _deliveryService.DeliverOrderAsync(user, deliverOrderRequest.DeliveryCode,
                deliverOrderRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<DeliverOrderResponse>(
                new DeliverOrderResponse("Order delivered successfully."),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while delivering order. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> DeliverOrderBack([FromQuery] DeliverOrderBackRequest deliverOrderBackRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.FiredEmployee - 1,
                cancellationToken);

            await _deliveryService.DeliverOrderBackAsync(user, deliverOrderBackRequest.DeliveryCode,
                deliverOrderBackRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<DeliverOrderBackResponse>(
                new DeliverOrderBackResponse("Order delivered back successfully."),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while delivering order back. " +
                                          $"--> {e.Message}");
        }
    }
}