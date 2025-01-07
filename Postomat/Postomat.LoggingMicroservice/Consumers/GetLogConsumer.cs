using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.LoggingMicroservice.Consumers;

public class GetLogConsumer : IConsumer<MicroserviceGetLogRequest>
{
    private readonly ILogsService _logsService;

    public GetLogConsumer(ILogsService logsService)
    {
        _logsService = logsService;
    }

    public async Task Consume(ConsumeContext<MicroserviceGetLogRequest> context)
    {
        try
        {
            var request = context.Message;

            var log = await _logsService.GetLogAsync(request.LogId, new CancellationToken());

            await context.RespondAsync(new MicroserviceGetLogResponse(
                new LogDto(log.Id, log.Date, log.Origin, log.Type, log.Title, log.Message),
                null));
        }
        catch (ExpectedException e) when (e is ServiceException)
        {
            await context.RespondAsync(new MicroserviceGetLogResponse(
                null,
                e.Message));
        }
        catch (ExpectedException e)
        {
            await context.RespondAsync(new MicroserviceGetLogResponse(
                null,
                $"Unexpected expected error. {e.Message}"));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceGetLogResponse(
                null,
                $"Unexpected unexpected error. {e.Message}"));
        }
    }
}