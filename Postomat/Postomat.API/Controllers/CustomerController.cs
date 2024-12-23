using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IControllerErrorLogService _controllerErrorLogService;

    public CustomerController(ICustomerService customerService,
        IControllerErrorLogService controllerErrorLogService)
    {
        _customerService = customerService;
        _controllerErrorLogService = controllerErrorLogService;
    }

    [HttpGet]
    public async Task<IActionResult> ReceiveOrder([FromQuery] ReceiveOrderRequest receiveOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.ReceiveOrderAsync(receiveOrderRequest.ReceivingCode,
                receiveOrderRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<ReceiveOrderResponse>(
                new ReceiveOrderResponse("Order received successfully"),
                null));
        }
        catch (Exception e)
        {
            return Ok(await _controllerErrorLogService.CreateErrorLog<ReceiveOrderResponse>(
                "Customer controller", "Error while receiving order", e.Message));
        }
    }
}