using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Postomat.API.Contracts.Requests;
using Postomat.API.Contracts.Responses;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Models;
using Postomat.Core.Models.Other;

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

    public AdminController(IAccessCheckService accessCheckService, IOrderPlansService orderPlansService,
        IOrdersService ordersService, IPostomatsService postomatsService, IRolesService rolesService,
        IUsersService usersService)
    {
        _accessCheckService = accessCheckService;
        _orderPlansService = orderPlansService;
        _ordersService = ordersService;
        _postomatsService = postomatsService;
        _rolesService = rolesService;
        _usersService = usersService;
    }

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

            return Redirect("/Customer/ReceiveOrder");
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while receiving order. " +
                                          $"--> {e.Message}");
        }
    }

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

            return Redirect("/Delivery/DeliverOrder");
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

            return Redirect("/Delivery/DeliverOrderBack");
        }
        catch (ServiceException e)
        {
            throw new ControllerException($"Error while delivering order back. " +
                                          $"--> {e.Message}");
        }
    }
}