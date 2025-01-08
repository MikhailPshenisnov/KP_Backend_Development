namespace Postomat.Core.Abstractions.Services;

public interface ICustomerService
{
    Task<List<Guid>> ReceiveOrderAsync(string receivingCode, Guid postomatId, CancellationToken ct);
}