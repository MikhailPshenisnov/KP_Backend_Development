using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models.Other;
using ConsumerException = Postomat.Core.Exceptions.BaseExceptions.ConsumerException;

namespace Postomat.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAccessCheckService _accessCheckService;
    private readonly IOrderPlansService _orderPlansService;
    private readonly IOrdersService _ordersService;
    private readonly IPostomatsService _postomatsService;
    private readonly IRolesService _rolesService;
    private readonly IUsersService _usersService;
    private readonly IRequestClient<MicroserviceCreateLogRequest> _createLogClient;
    private readonly IRequestClient<MicroserviceGetLogRequest> _getLogClient;
    private readonly IRequestClient<MicroserviceGetFilteredLogsRequest> _getFilteredLogsClient;
    private readonly IRequestClient<MicroserviceUpdateLogRequest> _updateLogClient;
    private readonly IRequestClient<MicroserviceDeleteLogRequest> _deleteLogClient;

    public AdminController(IAccessCheckService accessCheckService, IOrderPlansService orderPlansService,
        IOrdersService ordersService, IPostomatsService postomatsService, IRolesService rolesService,
        IUsersService usersService, IRequestClient<MicroserviceCreateLogRequest> createLogClient,
        IRequestClient<MicroserviceGetLogRequest> getLogClient,
        IRequestClient<MicroserviceGetFilteredLogsRequest> getFilteredLogsClient,
        IRequestClient<MicroserviceUpdateLogRequest> updateLogClient,
        IRequestClient<MicroserviceDeleteLogRequest> deleteLogClient)
    {
        _accessCheckService = accessCheckService;
        _orderPlansService = orderPlansService;
        _ordersService = ordersService;
        _postomatsService = postomatsService;
        _rolesService = rolesService;
        _usersService = usersService;
        _createLogClient = createLogClient;
        _getLogClient = getLogClient;
        _getFilteredLogsClient = getFilteredLogsClient;
        _updateLogClient = updateLogClient;
        _deleteLogClient = deleteLogClient;
    }

    /* Customer functionality */
    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> ReceiveOrder([FromQuery] ReceiveOrderRequest receiveOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var queryString = HttpContext.Request.QueryString.Value;
            return Redirect($"/Customer/ReceiveOrder{queryString}");
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while receiving order. " +
                                          $"--> {e.Message}");
        }
    }

    /* Delivery functionality */
    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> DeliverOrder([FromQuery] DeliverOrderRequest deliverOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var queryString = HttpContext.Request.QueryString.Value;
            return Redirect($"/Delivery/DeliverOrder{queryString}");
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while delivering order. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> DeliverOrderBack([FromQuery] DeliverOrderBackRequest deliverOrderBackRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var queryString = HttpContext.Request.QueryString.Value;
            return Redirect($"/Delivery/DeliverOrderBack{queryString}");
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while delivering order back. " +
                                          $"--> {e.Message}");
        }
    }

    /* Log management functionality */
    [Authorize]
    [HttpGet("Logs/[action]")]
    public async Task<IActionResult> GetLog([FromQuery] GetLogRequest getLogRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var microserviceResponse = (await _getLogClient
                    .GetResponse<MicroserviceGetLogResponse>(
                        new MicroserviceGetLogRequest(
                            getLogRequest.LogId),
                        cancellationToken))
                .Message;

            if (microserviceResponse.ErrorMessage is not null)
                throw new ConsumerException($"Unable to get log (microservice error). " +
                                            $"--> {microserviceResponse.ErrorMessage}");

            if (microserviceResponse.LogDto is null)
                throw new ConsumerException("If no errors occurred, the log cannot be empty.");

            return Ok(new BaseResponse<GetLogResponse>(
                new GetLogResponse(
                    microserviceResponse.LogDto.Id,
                    microserviceResponse.LogDto.Date,
                    microserviceResponse.LogDto.Origin,
                    microserviceResponse.LogDto.Type,
                    microserviceResponse.LogDto.Title,
                    microserviceResponse.LogDto.Message),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while getting log. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpGet("Logs/[action]")]
    public async Task<IActionResult> GetFilteredLogs([FromQuery] GetFilteredLogsRequest getFilteredLogsRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var microserviceResponse = (await _getFilteredLogsClient
                    .GetResponse<MicroserviceGetFilteredLogsResponse>(
                        new MicroserviceGetFilteredLogsRequest(
                            new LogFilterDto(
                                getFilteredLogsRequest.DateFrom,
                                getFilteredLogsRequest.DateTo,
                                getFilteredLogsRequest.PartOfOrigin,
                                getFilteredLogsRequest.PartOfType,
                                getFilteredLogsRequest.PartOfTitle,
                                getFilteredLogsRequest.PartOfMessage)),
                        cancellationToken))
                .Message;

            if (microserviceResponse.ErrorMessage is not null)
                throw new ConsumerException($"Unable to get filtered logs (microservice error). " +
                                            $"--> {microserviceResponse.ErrorMessage}");

            if (microserviceResponse.LogDtoList is null)
                throw new ConsumerException("If no errors occurred, the log list cannot be empty.");

            return Ok(new BaseResponse<List<GetFilteredLogsResponse>>(
                microserviceResponse.LogDtoList
                    .Select(l => new GetFilteredLogsResponse(
                        l.Id,
                        l.Date,
                        l.Origin,
                        l.Type,
                        l.Title,
                        l.Message))
                    .ToList(),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while getting filtered logs. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpPost("Logs/[action]")]
    public async Task<IActionResult> CreateLog([FromBody] CreateLogRequest createLogRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var microserviceResponse = (await _createLogClient
                    .GetResponse<MicroserviceCreateLogResponse>(
                        new MicroserviceCreateLogRequest(
                            new LogDto(
                                Guid.NewGuid(),
                                createLogRequest.Date,
                                createLogRequest.Origin,
                                createLogRequest.Type,
                                createLogRequest.Title,
                                createLogRequest.Message)),
                        cancellationToken))
                .Message;

            if (microserviceResponse.ErrorMessage is not null)
                throw new ConsumerException($"Unable to create log (microservice error). " +
                                            $"--> {microserviceResponse.ErrorMessage}");

            if (microserviceResponse.CreatedLogId is null)
                throw new ConsumerException("If no errors occurred, the created log id cannot be empty.");

            return Ok(new BaseResponse<CreateLogResponse>(
                new CreateLogResponse(
                    microserviceResponse.CreatedLogId.Value),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while creating log. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpPut("Logs/[action]")]
    public async Task<IActionResult> UpdateLog([FromBody] UpdateLogRequest updateLogRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var microserviceResponse = (await _updateLogClient
                    .GetResponse<MicroserviceUpdateLogResponse>(
                        new MicroserviceUpdateLogRequest(
                            updateLogRequest.LogId,
                            new LogDto(
                                updateLogRequest.LogId,
                                updateLogRequest.NewLogDate,
                                updateLogRequest.NewLogOrigin,
                                updateLogRequest.NewLogType,
                                updateLogRequest.NewLogTitle,
                                updateLogRequest.NewLogMessage)),
                        cancellationToken))
                .Message;

            if (microserviceResponse.ErrorMessage is not null)
                throw new ConsumerException($"Unable to update log (microservice error). " +
                                            $"--> {microserviceResponse.ErrorMessage}");

            if (microserviceResponse.UpdatedLogId is null)
                throw new ConsumerException("If no errors occurred, the updated log id cannot be empty.");

            return Ok(new BaseResponse<UpdateLogResponse>(
                new UpdateLogResponse(
                    microserviceResponse.UpdatedLogId.Value),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while updating log. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpDelete("Logs/[action]")]
    public async Task<IActionResult> DeleteLog([FromQuery] DeleteLogRequest deleteLogRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.JuniorAdministrator - 1,
                cancellationToken);

            var microserviceResponse = (await _deleteLogClient
                    .GetResponse<MicroserviceDeleteLogResponse>(
                        new MicroserviceDeleteLogRequest(
                            deleteLogRequest.LogId),
                        cancellationToken))
                .Message;

            if (microserviceResponse.ErrorMessage is not null)
                throw new ConsumerException($"Unable to delete log (microservice error). " +
                                            $"--> {microserviceResponse.ErrorMessage}");

            if (microserviceResponse.DeletedLogId is null)
                throw new ConsumerException("If no errors occurred, the deleted log id cannot be empty.");

            return Ok(new BaseResponse<DeleteLogResponse>(
                new DeleteLogResponse(
                    microserviceResponse.DeletedLogId.Value),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while deleting log. " +
                                          $"--> {e.Message}");
        }
    }
    
    /* Orders management functionality */
    
    /* Order plans management functionality */
    
    /* Postomats management functionality */
    
    /* Roles management functionality */
    
    /* Users management functionality */
}