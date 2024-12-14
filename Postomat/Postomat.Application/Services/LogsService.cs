using Postomat.Core.Abstractions.Repositories;

namespace Postomat.Application.Services;

public class LogsService
{
    private readonly ILogsRepository _postomatsRepository;

    public LogsService(ILogsRepository postomatsRepository)
    {
        _postomatsRepository = postomatsRepository;
    }
}