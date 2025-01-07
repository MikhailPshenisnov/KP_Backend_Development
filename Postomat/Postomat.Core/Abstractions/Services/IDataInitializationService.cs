namespace Postomat.Core.Abstractions.Services;

public interface IDataInitializationService
{
    Task InitializeData(CancellationToken ct);
}