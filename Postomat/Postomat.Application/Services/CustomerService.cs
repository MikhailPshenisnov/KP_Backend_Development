using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions.ControllerExceptions;
using Postomat.Core.Models;

namespace Postomat.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IPostomatsService _postomatsService;

    public CustomerService(IPostomatsService postomatsService)
    {
        _postomatsService = postomatsService;
    }

    public async Task<List<Guid>> ReceiveOrderAsync(string receivingCode, Guid postomatId, CancellationToken ct)
    {
        try
        {
            var postomat = await _postomatsService.GetPostomatAsync(postomatId, ct);

            var cellsWithOrder = new List<Cell>();
            foreach (var cell in postomat.Cells)
            {
                if (cell.Order is null) continue;
                if (BCrypt.Net.BCrypt.EnhancedVerify(receivingCode, cell.Order.ReceivingCodeHash)) 
                    cellsWithOrder.Add(cell);
            }
            if (cellsWithOrder.Count == 0)
                throw new ReceivingException($"Unknown receiving code: \"{receivingCode}\".");

            foreach (var cell in cellsWithOrder)
                await _postomatsService.ClearCellInPostomatAsync(null, postomatId, cell.Order!, ct);

            return cellsWithOrder.Select(c => c.Id).ToList();
        }
        catch (ServiceException e)
        {
            throw new ServiceException($"Unable to receive order. " +
                                       $"--> {e.Message}");
        }
    }
}