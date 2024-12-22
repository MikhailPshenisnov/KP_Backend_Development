using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.LoggingMicroservice.Consumers;

public class DeleteLogConsumer : IConsumer<MicroserviceDeleteLogRequest>
{
    private readonly ILogsService _logsService;

    public DeleteLogConsumer(ILogsService logsService)
    {
        _logsService = logsService;
    }

    public async Task Consume(ConsumeContext<MicroserviceDeleteLogRequest> context)
    {
        try
        {
            var request = context.Message;
            var deletedLogId = await _logsService.DeleteLogAsync(request.LogId, new CancellationToken());

            await context.RespondAsync(new MicroserviceDeleteLogResponse(
                deletedLogId,
                null));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceDeleteLogResponse(
                null,
                e.Message));
        }
    }
}