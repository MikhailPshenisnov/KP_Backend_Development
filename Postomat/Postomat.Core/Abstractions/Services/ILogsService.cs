using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Core.Abstractions.Services;

public interface ILogsService
{
    Task<Guid> CreateLogAsync(Log log, CancellationToken ct);
    Task<Log> GetLogAsync(Guid logId, CancellationToken ct);
    Task<List<Log>> GetFilteredLogsAsync(LogFilter? logFilter, CancellationToken ct);
    Task<Guid> UpdateLogAsync(Guid logId, Log newLog, CancellationToken ct);
    Task<Guid> DeleteLogAsync(Guid logId, CancellationToken ct);
}