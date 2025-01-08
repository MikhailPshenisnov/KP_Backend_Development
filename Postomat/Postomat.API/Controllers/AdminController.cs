using MassTransit;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;
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
    [Authorize]
    [HttpGet("Orders/[action]")]
    public async Task<IActionResult> GetOrder([FromQuery] GetOrderRequest getOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var order = await _ordersService
                .GetOrderAsync(getOrderRequest.OrderId, cancellationToken);

            return Ok(new BaseResponse<GetOrderResponse>(
                new GetOrderResponse(
                    order.Id,
                    order.ReceivingCodeHash,
                    order.OrderSize),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while getting order. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpGet("Orders/[action]")]
    public async Task<IActionResult> GetFilteredOrders([FromQuery] GetFilteredOrdersRequest getFilteredOrdersRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var (orderFilter, error) = getFilteredOrdersRequest.OrderSizeFrom is not null ||
                                       getFilteredOrdersRequest.OrderSizeTo is not null
                ? OrderFilter.Create(
                    getFilteredOrdersRequest.OrderSizeFrom,
                    getFilteredOrdersRequest.OrderSizeTo)
                : (null, string.Empty);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert order filter dto to order filter model. " +
                                              $"--> {error}");

            var orders = await _ordersService
                .GetFilteredOrdersAsync(orderFilter, cancellationToken);

            return Ok(new BaseResponse<List<GetFilteredOrdersResponse>>(
                orders
                    .Select(o => new GetFilteredOrdersResponse(
                        o.Id,
                        o.ReceivingCodeHash,
                        o.OrderSize))
                    .ToList(),
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            throw new ControllerException($"Error while getting filtered orders. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpPost("Orders/[action]")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest createOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var (order, error) = Order.Create(
                Guid.NewGuid(),
                BCrypt.Net.BCrypt.EnhancedHashPassword(createOrderRequest.ReceivingCode),
                createOrderRequest.OrderSize);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert order dto to order model. " +
                                              $"--> {error}");

            var createdOrderId = await _ordersService
                .CreateOrderAsync(order, cancellationToken);

            return Ok(new BaseResponse<CreateOrderResponse>(
                new CreateOrderResponse(
                    createdOrderId),
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            throw new ControllerException($"Error while creating order. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpPut("Orders/[action]")]
    public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequest updateOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var (newOrder, error) = Order.Create(
                updateOrderRequest.OrderId,
                BCrypt.Net.BCrypt.EnhancedHashPassword(updateOrderRequest.NewOrderReceivingCode),
                updateOrderRequest.NewOrderOrderSize);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert order dto to order model. " +
                                              $"--> {error}");

            var updatedOrderId = await _ordersService
                .UpdateOrderAsync(updateOrderRequest.OrderId, newOrder, cancellationToken);

            return Ok(new BaseResponse<UpdateOrderResponse>(
                new UpdateOrderResponse(
                    updatedOrderId),
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            throw new ControllerException($"Error while updating order. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpDelete("Orders/[action]")]
    public async Task<IActionResult> DeleteOrder([FromQuery] DeleteOrderRequest deleteOrderRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.JuniorAdministrator - 1,
                cancellationToken);

            var deletedOrderId = await _ordersService
                .DeleteOrderAsync(deleteOrderRequest.OrderId, cancellationToken);

            return Ok(new BaseResponse<DeleteOrderResponse>(
                new DeleteOrderResponse(
                    deletedOrderId),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while deleting order. " +
                                          $"--> {e.Message}");
        }
    }

    /* Order plans management functionality */
    [Authorize]
    [HttpGet("OrderPlans/[action]")]
    public async Task<IActionResult> GetOrderPlan([FromQuery] GetOrderPlanRequest getOrderPlanRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var orderPlan = await _orderPlansService
                .GetOrderPlanAsync(getOrderPlanRequest.OrderPlanId, cancellationToken);

            return Ok(new BaseResponse<GetOrderPlanResponse>(
                new GetOrderPlanResponse(
                    orderPlan.Id,
                    orderPlan.Status,
                    orderPlan.LastStatusChangeDate,
                    orderPlan.StoreUntilDate,
                    orderPlan.DeliveryCodeHash,
                    orderPlan.Order.Id,
                    orderPlan.Postomat.Id,
                    orderPlan.CreatedBy.Id,
                    orderPlan.DeliveredBy?.Id,
                    orderPlan.DeliveredBackBy?.Id),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while getting order plan. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpGet("OrderPlans/[action]")]
    public async Task<IActionResult> GetFilteredOrderPlans(
        [FromQuery] GetFilteredOrderPlansRequest getFilteredOrderPlansRequest, CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var (orderPlanFilter, error) = getFilteredOrderPlansRequest.PartOfStatus is not null ||
                                           getFilteredOrderPlansRequest.LastStatusChangeDateFrom is not null ||
                                           getFilteredOrderPlansRequest.LastStatusChangeDateTo is not null ||
                                           getFilteredOrderPlansRequest.StoreUntilDateFrom is not null ||
                                           getFilteredOrderPlansRequest.StoreUntilDateTo is not null ||
                                           getFilteredOrderPlansRequest.OrderId is not null ||
                                           getFilteredOrderPlansRequest.PostomatId is not null ||
                                           getFilteredOrderPlansRequest.UserId is not null
                ? OrderPlanFilter.Create(
                    getFilteredOrderPlansRequest.PartOfStatus,
                    getFilteredOrderPlansRequest.LastStatusChangeDateFrom,
                    getFilteredOrderPlansRequest.LastStatusChangeDateTo,
                    getFilteredOrderPlansRequest.StoreUntilDateFrom,
                    getFilteredOrderPlansRequest.StoreUntilDateTo,
                    getFilteredOrderPlansRequest.OrderId,
                    getFilteredOrderPlansRequest.PostomatId,
                    getFilteredOrderPlansRequest.UserId)
                : (null, string.Empty);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert order plan filter dto to order plan filter model. " +
                                              $"--> {error}");

            var orderPlans = await _orderPlansService
                .GetFilteredOrderPlansAsync(orderPlanFilter, cancellationToken);

            return Ok(new BaseResponse<List<GetFilteredOrderPlansResponse>>(
                orderPlans
                    .Select(op => new GetFilteredOrderPlansResponse(
                        op.Id,
                        op.Status,
                        op.LastStatusChangeDate,
                        op.StoreUntilDate,
                        op.DeliveryCodeHash,
                        op.Order.Id,
                        op.Postomat.Id,
                        op.CreatedBy.Id,
                        op.DeliveredBy?.Id,
                        op.DeliveredBackBy?.Id))
                    .ToList(),
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            throw new ControllerException($"Error while getting filtered order plans. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpPost("OrderPlans/[action]")]
    public async Task<IActionResult> CreateOrderPlan([FromBody] CreateOrderPlanRequest createOrderPlanRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var order = await _ordersService
                .GetOrderAsync(createOrderPlanRequest.OrderId, cancellationToken);
            var postomat = await _postomatsService
                .GetPostomatAsync(createOrderPlanRequest.PostomatId, cancellationToken);

            var (orderPlan, error) = OrderPlan.Create(
                Guid.NewGuid(),
                "Created",
                DateTime.Now.ToUniversalTime(),
                null,
                BCrypt.Net.BCrypt.EnhancedHashPassword(createOrderPlanRequest.DeliveryCode),
                order,
                postomat,
                user,
                null,
                null);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert order plan dto to order plan model. " +
                                              $"--> {error}");

            var createdOrderPlanId = await _orderPlansService
                .CreateOrderPlanAsync(orderPlan, cancellationToken);

            return Ok(new BaseResponse<CreateOrderPlanResponse>(
                new CreateOrderPlanResponse(
                    createdOrderPlanId),
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            throw new ControllerException($"Error while creating order plan. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpPut("OrderPlans/[action]")]
    public async Task<IActionResult> UpdateOrderPlan([FromBody] UpdateOrderPlanRequest updateOrderPlanRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.DeliveryMan - 1,
                cancellationToken);

            var oldOrderPlan = await _orderPlansService
                .GetOrderPlanAsync(updateOrderPlanRequest.OrderPlanId, cancellationToken);

            var (newOrderPlan, error) = OrderPlan.Create(
                updateOrderPlanRequest.OrderPlanId,
                oldOrderPlan.Status,
                oldOrderPlan.LastStatusChangeDate,
                oldOrderPlan.StoreUntilDate,
                BCrypt.Net.BCrypt.EnhancedHashPassword(updateOrderPlanRequest.NewOrderPlanDeliveryCode),
                oldOrderPlan.Order,
                oldOrderPlan.Postomat,
                oldOrderPlan.CreatedBy,
                oldOrderPlan.DeliveredBy,
                oldOrderPlan.DeliveredBackBy);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert order plan dto to order plan model. " +
                                              $"--> {error}");

            var updatedOrderPlanId = await _orderPlansService
                .UpdateOrderPlanAsync(updateOrderPlanRequest.OrderPlanId, newOrderPlan, cancellationToken);

            return Ok(new BaseResponse<UpdateOrderPlanResponse>(
                new UpdateOrderPlanResponse(
                    updatedOrderPlanId),
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            throw new ControllerException($"Error while updating order plan. " +
                                          $"--> {e.Message}");
        }
    }

    [Authorize]
    [HttpDelete("OrderPlans/[action]")]
    public async Task<IActionResult> DeleteOrderPlan([FromQuery] DeleteOrderPlanRequest deleteOrderPlanRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessCheckService.CheckAccessLvl(
                HttpContext.Request,
                (int)AccessLvlEnumerator.JuniorAdministrator - 1,
                cancellationToken);

            var deletedOrderPlanId = await _orderPlansService
                .DeleteOrderPlanAsync(deleteOrderPlanRequest.OrderPlanId, cancellationToken);

            return Ok(new BaseResponse<DeleteOrderPlanResponse>(
                new DeleteOrderPlanResponse(
                    deletedOrderPlanId),
                null));
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while deleting order plan. " +
                                          $"--> {e.Message}");
        }
    }

    /* Postomats management functionality */

    /* Roles management functionality */

    /* Users management functionality */
}