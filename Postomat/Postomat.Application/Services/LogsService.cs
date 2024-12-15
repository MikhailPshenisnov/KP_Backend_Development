using Microsoft.Extensions.Logging;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Models;
using Postomat.Core.Models.Filters;

namespace Postomat.Application.Services;

public class LogsService : ILogsService
{
    private readonly ILogsRepository _logsRepository;
    private readonly ILogger _logger;

    public LogsService(ILogsRepository logsRepository, ILogger<LogsService> logger)
    {
        _logsRepository = logsRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateLogAsync(Log log, CancellationToken ct)
    {
        try
        {
            var createdLogId = await _logsRepository.CreateLog(log);

            if (log.Type == "Error")
                _logger.LogError("{logDate} ({logType} log from {logOrigin}) \"{logTitle}\":\n{logMessage}",
                    log.Date, log.Type, log.Origin, log.Title, log.Message);
            else
                _logger.LogInformation("{logDate} ({logType} log from {logOrigin}) \"{logTitle}\":\n{logMessage}",
                    log.Date, log.Type, log.Origin, log.Title, log.Message);

            return createdLogId;
        }
        catch (Exception e)
        {
            _logger.LogError("Something went wrong during the log creation process, " +
                             "the log was not created: {message}", e.Message);
            throw new Exception($"Unable to create log \"{log.Id}\": \"{e.Message}\"");
        }
    }

    public async Task<Log> GetLogAsync(Guid logId, CancellationToken ct)
    {
        try
        {
            var allLogs = await _logsRepository.GetAllLogs();

            var log = allLogs.FirstOrDefault(l => l.Id == logId);
            if (log == null)
                throw new Exception($"Unknown log id: \"{logId}\"");

            return log;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to get log \"{logId}\": \"{e.Message}\"");
        }
    }

    public async Task<List<Log>> GetFilteredLogsAsync(LogFilter? logFilter, CancellationToken ct)
    {
        try
        {
            var logs = await _logsRepository.GetAllLogs();

            if (logFilter == null)
                return logs;

            if (logFilter.DateFrom is not null)
            {
                logs = logs
                    .Where(l => l.Date >= logFilter.DateFrom?.ToUniversalTime())
                    .ToList();
            }

            if (logFilter.DateTo is not null)
            {
                logs = logs
                    .Where(l => l.Date <= logFilter.DateTo?.ToUniversalTime())
                    .ToList();
            }

            if (logFilter.PartOfOrigin is not null)
            {
                logs = logs
                    .Where(l => l.Origin.ToLower().Contains(logFilter.PartOfOrigin.ToLower()))
                    .ToList();
            }

            if (logFilter.PartOfType is not null)
            {
                logs = logs
                    .Where(l => l.Origin.ToLower().Contains(logFilter.PartOfType.ToLower()))
                    .ToList();
            }

            if (logFilter.PartOfTitle is not null)
            {
                logs = logs
                    .Where(l => l.Origin.ToLower().Contains(logFilter.PartOfTitle.ToLower()))
                    .ToList();
            }

            if (logFilter.PartOfMessage is not null)
            {
                logs = logs
                    .Where(l => l.Origin.ToLower().Contains(logFilter.PartOfMessage.ToLower()))
                    .ToList();
            }

            return logs.OrderBy(x => x.Date).ToList();
        }

        catch (Exception e)
        {
            throw new Exception($"Unable to get filtered logs: \"{e.Message}\"");
        }
    }

    public async Task<Guid> UpdateLogAsync(Guid logId, Log newLog, CancellationToken ct)
    {
        try
        {
            var updatedLogId = await _logsRepository.UpdateLog(logId, newLog);

            return updatedLogId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to update log \"{logId}\": \"{e.Message}\"");
        }
    }

    public async Task<Guid> DeleteLogAsync(Guid logId, CancellationToken ct)
    {
        try
        {
            var existedLogId = await _logsRepository.DeleteLog(logId);

            return existedLogId;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to delete log \"{logId}\": \"{e.Message}\"");
        }
    }
}