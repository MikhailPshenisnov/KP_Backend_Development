using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.LoggingMicroservice.Consumers;

public class CreateLogConsumer : IConsumer<MicroserviceCreateLogRequest>
{
    private readonly ILogsService _logsService;

    public CreateLogConsumer(ILogsService logsService)
    {
        _logsService = logsService;
    }

    public async Task Consume(ConsumeContext<MicroserviceCreateLogRequest> context)
    {
        try
        {
            var request = context.Message;
            var createdLogId = await _logsService.CreateLogAsync(request.Log, new CancellationToken());

            await context.RespondAsync(new MicroserviceCreateLogResponse(
                createdLogId,
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