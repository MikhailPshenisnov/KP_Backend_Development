using Microsoft.EntityFrameworkCore;
using Postomat.Core.Abstractions.Repositories;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.Models;
using Postomat.DataAccess.Database.Context;

namespace Postomat.DataAccess.Repositories;

public class LogsRepository : ILogsRepository
{
    private readonly PostomatDbContext _context;

    public LogsRepository(PostomatDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateLog(Log log)
    {
        var logEntity = new Database.Entities.Log
        {
            Id = log.Id,
            Date = log.Date,
            Origin = log.Origin,
            Type = log.Type,
            Title = log.Title,
            Message = log.Message
        };

        await _context.Logs.AddAsync(logEntity);
        await _context.SaveChangesAsync();

        return logEntity.Id;
    }

    public async Task<List<Log>> GetAllLogs()
    {
        var logEntities = await _context.Logs
            .AsNoTracking()
            .ToListAsync();

        var logs = logEntities
            .Select(logEntity =>
            {
                var (logModel, logError) = Log
                    .Create(
                        logEntity.Id,
                        logEntity.Date,
                        logEntity.Origin,
                        logEntity.Type,
                        logEntity.Title,
                        logEntity.Message);
                if (!string.IsNullOrEmpty(logError))
                    throw new ConversionException($"Unable to convert log entity to log model. " +
                                                  $"--> {logError}");

                return logModel;
            })
            .ToList();

        return logs;
    }

    public async Task<Guid> UpdateLog(Guid logId, Log newLog)
    {
        var oldLog = (await GetAllLogs())
            .FirstOrDefault(l => l.Id == logId);
        if (oldLog is null)
            throw new UnknownIdentifierException($"Unknown log id: \"{logId}\".");

        await _context.Logs
            .Where(l => l.Id == logId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(l => l.Date, l => newLog.Date)
                .SetProperty(l => l.Origin, l => newLog.Origin)
                .SetProperty(l => l.Type, l => newLog.Type)
                .SetProperty(l => l.Title, l => newLog.Title)
                .SetProperty(l => l.Message, l => newLog.Message));

        return logId;
    }

    public async Task<Guid> DeleteLog(Guid logId)
    {
        var numUpdated = await _context.Logs
            .Where(l => l.Id == logId)
            .ExecuteDeleteAsync();

        if (numUpdated == 0)
            throw new UnknownIdentifierException($"Unknown log id: \"{logId}\".");

        return logId;
    }
}