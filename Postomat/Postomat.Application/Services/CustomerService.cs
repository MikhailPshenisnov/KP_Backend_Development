using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions.ControllerExceptions;

namespace Postomat.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IPostomatsService _postomatsService;

    public CustomerService(IPostomatsService postomatsService)
    {
        _postomatsService = postomatsService;
    }

    public async Task<Guid> ReceiveOrderAsync(string receivingCode, Guid postomatId, CancellationToken ct)
    {
        try
        {
            var postomat = await _postomatsService.GetPostomatAsync(postomatId, ct);

            var cellsWithOrder = postomat.Cells
                .Where(c => BCrypt.Net.BCrypt
                    .EnhancedVerify(receivingCode, c.Order?.ReceivingCodeHash ?? string.Empty))
                .ToList();
            if (cellsWithOrder.Count == 0)
                throw new ReceivingException($"Unknown receiving code: \"{receivingCode}\".");

            foreach (var cell in cellsWithOrder)
                await _postomatsService.ClearCellInPostomatAsync(null, postomatId, cell.Order!, ct);

            return cellsWithOrder[0].Id;
        }
        catch (ServiceException e)
        {
            throw new ServiceException($"Unable to receive order. " +
                                       $"--> {e.Message}");
        }
    }
}