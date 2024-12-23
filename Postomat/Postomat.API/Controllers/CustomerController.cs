using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IRequestClient<MicroserviceCreateLogRequest> _createLogClient;

    public CustomerController(ICustomerService customerService,
        IRequestClient<MicroserviceCreateLogRequest> createLogClient)
    {
        _customerService = customerService;
        _createLogClient = createLogClient;
    }

    [HttpGet]
    public async Task<IActionResult> ReceiveOrder([FromQuery] ReceiveOrderRequest receiveOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.ReceiveOrderAsync(receiveOrderRequest.ReceivingCode,
                receiveOrderRequest.PostomatId, cancellationToken);

            return Ok(new BaseResponse<ReceiveOrderResponse>
            (
                new ReceiveOrderResponse("Order received successfully"),
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
                    "Customer controller",
                    "Error",
                    "Error while receiving order",
                    e.Message);
                if (error is not null)
                    throw new Exception($"Unable to create error log: {error}");

                var response = await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                    new MicroserviceCreateLogRequest(log)
                );
                if (response.Message.ErrorMessage is not null)
                    throw new Exception($"Unable to create error log (microservice error): {error}");

                return Ok(new BaseResponse<ReceiveOrderResponse>(
                    null,
                    e.Message + $" Error log was created: \"{response.Message.CreatedLogId}\""
                ));
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponse<ReceiveOrderResponse>(
                    null,
                    e.Message + $" Error log was not created: \"{ex.Message}\""
                ));
            }
        }
    }
}