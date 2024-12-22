using Postomat.Core.Abstractions.Services;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Application.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IPostomatsService _postomatsService;
    private readonly IOrderPlansService _orderPlansService;
    private readonly IOrdersService _ordersService;

    public DeliveryService(IPostomatsService postomatsService, IOrderPlansService orderPlansService,
        IOrdersService ordersService)
    {
        _postomatsService = postomatsService;
        _orderPlansService = orderPlansService;
        _ordersService = ordersService;
    }

    public async Task<Guid> DeliverOrderAsync(User user, string deliveryCode, Guid postomatId, CancellationToken ct)
    {
        try
        {
            var postomat = await _postomatsService.GetPostomatAsync(postomatId, ct);
            if (postomat is null)
                throw new Exception($"Unknown postomat id \"{postomatId}\"");

            var filter = OrderPlanFilter
                .Create(null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    postomat.Id,
                    null);
            if (filter.Error is not null)
                throw new Exception($"Unable to create filter for order plan: {filter.Error}");

            var orderIds = (await _orderPlansService
                    .GetFilteredOrderPlansAsync(filter.OrderPlanFilter, ct))
                .Where(op => BCrypt.Net.BCrypt.EnhancedVerify(deliveryCode, op.DeliveryCodeHash))
                .Select(op => op.Order.Id)
                .ToList();
            if (orderIds.Count == 0)
                throw new Exception($"Unknown delivery code \"{deliveryCode}\"");

            foreach (var orderId in orderIds)
            {
                var order = await _ordersService.GetOrderAsync(orderId, ct);
                await _postomatsService.FillCellInPostomatAsync(user, postomatId, order, ct);
            }

            return orderIds[0];
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to deliver order: {e.Message}");
        }
    }

    public async Task<Guid> DeliverOrderBackAsync(User user, string deliveryCode, Guid postomatId, CancellationToken ct)
    {
        try
        {
            var postomat = await _postomatsService.GetPostomatAsync(postomatId, ct);
            if (postomat is null)
                throw new Exception($"Unknown postomat id \"{postomatId}\"");

            var filter = OrderPlanFilter
                .Create(null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    postomat.Id,
                    null);
            if (filter.Error is not null)
                throw new Exception($"Unable to create filter for order plan: {filter.Error}");

            var orderIds = (await _orderPlansService
                    .GetFilteredOrderPlansAsync(filter.OrderPlanFilter, ct))
                .Where(op => BCrypt.Net.BCrypt.EnhancedVerify(deliveryCode, op.DeliveryCodeHash) &&
                             op.StoreUntilDate < DateTime.Now.ToUniversalTime())
                .Select(op => op.Order.Id)
                .ToList();
            if (orderIds.Count == 0)
                throw new Exception($"Unknown delivery code \"{deliveryCode}\"");

            foreach (var orderId in orderIds)
            {
                var order = await _ordersService.GetOrderAsync(orderId, ct);
                await _postomatsService.ClearCellInPostomatAsync(user, postomatId, order, ct);
            }

            return orderIds[0];
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to deliver order: {e.Message}");
        }
    }
}