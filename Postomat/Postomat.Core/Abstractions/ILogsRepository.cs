using Postomat.Core.Models;

namespace Postomat.Core.Abstractions;

public interface ILogsRepository
{
    Task<Guid> CreateLog(Log log);
    Task<List<Log>> GetAllLogs();
    Task<Guid> UpdateLog(Guid logId, Log newLog);
    Task<Guid> DeleteLog(Guid logId);
}