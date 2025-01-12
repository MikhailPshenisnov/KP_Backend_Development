using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;
using Postomat.Core.Exceptions.BaseExceptions;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
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
                new ReceiveOrderResponse("Order received successfully."),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while receiving order. " +
                                          $"--> {e.Message}");
        }
    }
}