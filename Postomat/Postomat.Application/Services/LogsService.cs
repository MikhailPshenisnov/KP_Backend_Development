using Microsoft.Extensions.Logging;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
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

            if (log.Type.Contains("exception", StringComparison.CurrentCultureIgnoreCase) ||
                log.Type.Contains("error", StringComparison.CurrentCultureIgnoreCase))
                _logger.LogError("{LogDate} ({LogType} log from {LogOrigin}) \"{LogTitle}\":\n{LogMessage}",
                    log.Date, log.Type, log.Origin, log.Title, log.Message);
            else
                _logger.LogInformation("{LogDate} ({LogType} log from {LogOrigin}) \"{LogTitle}\":\n{LogMessage}",
                    log.Date, log.Type, log.Origin, log.Title, log.Message);

            return createdLogId;
        }
        catch (RepositoryException e)
        {
            _logger.LogError("Something went wrong during the log creation process, " +
                             "the log was not created: {Message}", e.Message);
            throw new ServiceException($"Unable to create log \"{log.Id}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Log> GetLogAsync(Guid logId, CancellationToken ct)
    {
        try
        {
            var allLogs = await _logsRepository.GetAllLogs();

            var log = allLogs.FirstOrDefault(l => l.Id == logId);
            if (log == null)
                throw new UnknownIdentifierException($"Unknown log id: \"{logId}\".");

            return log;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to get log \"{logId}\". " +
                                       $"--> {e.Message}");
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
                    .Where(l => l.Origin
                        .Contains(logFilter.PartOfOrigin, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }

            if (logFilter.PartOfType is not null)
            {
                logs = logs
                    .Where(l => l.Origin
                        .Contains(logFilter.PartOfType, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }

            if (logFilter.PartOfTitle is not null)
            {
                logs = logs
                    .Where(l => l.Origin
                        .Contains(logFilter.PartOfTitle, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }

            if (logFilter.PartOfMessage is not null)
            {
                logs = logs
                    .Where(l => l.Origin
                        .Contains(logFilter.PartOfMessage, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }

            return logs.OrderBy(x => x.Date).ToList();
        }

        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to get filtered logs. " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> UpdateLogAsync(Guid logId, Log newLog, CancellationToken ct)
    {
        try
        {
            var updatedLogId = await _logsRepository.UpdateLog(logId, newLog);

            return updatedLogId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to update log \"{logId}\". " +
                                       $"--> {e.Message}");
        }
    }

    public async Task<Guid> DeleteLogAsync(Guid logId, CancellationToken ct)
    {
        try
        {
            var existedLogId = await _logsRepository.DeleteLog(logId);

            return existedLogId;
        }
        catch (RepositoryException e)
        {
            throw new ServiceException($"Unable to delete log \"{logId}\". " +
                                       $"--> {e.Message}");
        }
    }
}