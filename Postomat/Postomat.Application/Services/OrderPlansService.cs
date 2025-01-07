using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Application.Services;

public class OrderPlansService : IOrderPlansService
{
    private readonly IOrderPlansRepository _orderPlansRepository;

    public OrderPlansService(IOrderPlansRepository orderPlansRepository)
    {
        _orderPlansRepository = orderPlansRepository;
    }

    public async Task<Guid> CreateOrderPlanAsync(OrderPlan orderPlan, CancellationToken ct)
    {
        try
        {
            var createdOrderPlanId = await _orderPlansRepository.CreateOrderPlan(orderPlan);

            return createdOrderPlanId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to create order plan \"{orderPlan.Id}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<OrderPlan> GetOrderPlanAsync(Guid orderPlanId, CancellationToken ct)
    {
        try
        {
            var allOrderPlans = await _orderPlansRepository.GetAllOrderPlans();

            var orderPlan = allOrderPlans.FirstOrDefault(op => op.Id == orderPlanId);
            if (orderPlan == null)
                throw new UnknownIdentifierException($"Unknown order plan id: \"{orderPlanId}\".");

            return orderPlan;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to get order plan \"{orderPlanId}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<List<OrderPlan>> GetFilteredOrderPlansAsync(OrderPlanFilter? orderPlanFilter,
        CancellationToken ct)
    {
        try
        {
            var orderPlans = await _orderPlansRepository.GetAllOrderPlans();

            if (orderPlanFilter == null)
                return orderPlans;

            if (orderPlanFilter.PartOfStatus is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.Status.ToLower().Contains(orderPlanFilter.PartOfStatus.ToLower()))
                    .ToList();
            }

            if (orderPlanFilter.LastStatusChangeDateFrom is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.LastStatusChangeDate >= orderPlanFilter.LastStatusChangeDateFrom?.ToUniversalTime())
                    .ToList();
            }

            if (orderPlanFilter.LastStatusChangeDateTo is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.LastStatusChangeDate <= orderPlanFilter.LastStatusChangeDateTo?.ToUniversalTime())
                    .ToList();
            }

            if (orderPlanFilter.StoreUntilDateFrom is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.StoreUntilDate >= orderPlanFilter.StoreUntilDateFrom?.ToUniversalTime())
                    .ToList();
            }

            if (orderPlanFilter.StoreUntilDateTo is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.StoreUntilDate <= orderPlanFilter.StoreUntilDateTo?.ToUniversalTime())
                    .ToList();
            }

            if (orderPlanFilter.OrderId is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.Order.Id == orderPlanFilter.OrderId)
                    .ToList();
            }

            if (orderPlanFilter.PostomatId is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.Postomat.Id == orderPlanFilter.PostomatId)
                    .ToList();
            }

            if (orderPlanFilter.UserId is not null)
            {
                orderPlans = orderPlans
                    .Where(op => op.CreatedBy.Id == orderPlanFilter.UserId ||
                                 op.DeliveredBy?.Id == orderPlanFilter.UserId ||
                                 op.DeliveredBackBy?.Id == orderPlanFilter.UserId)
                    .ToList();
            }

            return orderPlans.OrderBy(x => x.LastStatusChangeDate).ToList();
        }

        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to get filtered order plans. " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> UpdateOrderPlanAsync(Guid orderPlanId, OrderPlan newOrderPlan, CancellationToken ct)
    {
        try
        {
            var updatedOrderPlanId = await _orderPlansRepository.UpdateOrderPlan(orderPlanId, newOrderPlan);

            return updatedOrderPlanId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to update order plan \"{orderPlanId}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> DeleteOrderPlanAsync(Guid orderPlanId, CancellationToken ct)
    {
        try
        {
            var existedOrderPlanId = await _orderPlansRepository.DeleteOrderPlan(orderPlanId);

            return existedOrderPlanId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to delete order plan \"{orderPlanId}\". " +
                                       $"--> {e.Message}");
        }
    }
}