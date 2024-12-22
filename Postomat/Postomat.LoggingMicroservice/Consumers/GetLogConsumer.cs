using MassTransit;
using Postomat.Core.Abstractions.Services;
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
                log,
                null));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceCreateLogResponse(
                null,
                e.Message));
        }
    }
}