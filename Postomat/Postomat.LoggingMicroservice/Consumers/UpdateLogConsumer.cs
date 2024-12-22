using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.LoggingMicroservice.Consumers;

public class UpdateLogConsumer : IConsumer<MicroserviceUpdateLogRequest>
{
    private readonly ILogsService _logsService;

    public UpdateLogConsumer(ILogsService logsService)
    {
        _logsService = logsService;
    }

    public async Task Consume(ConsumeContext<MicroserviceUpdateLogRequest> context)
    {
        try
        {
            var request = context.Message;
            var updatedLogId = await _logsService.UpdateLogAsync(request.LogId, request.NewLog,
                new CancellationToken());

            await context.RespondAsync(new MicroserviceUpdateLogResponse(
                updatedLogId,
                null));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceUpdateLogResponse(
                null,
                e.Message));
        }
    }
}