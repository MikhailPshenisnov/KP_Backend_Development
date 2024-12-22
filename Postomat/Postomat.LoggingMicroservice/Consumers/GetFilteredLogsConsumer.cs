using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.LoggingMicroservice.Consumers;

public class GetFilteredLogsConsumer : IConsumer<MicroserviceGetFilteredLogsRequest>
{
    private readonly ILogsService _logsService;

    public GetFilteredLogsConsumer(ILogsService logsService)
    {
        _logsService = logsService;
    }

    public async Task Consume(ConsumeContext<MicroserviceGetFilteredLogsRequest> context)
    {
        try
        {
            var request = context.Message;
            var logs = await _logsService.GetFilteredLogsAsync(request.LogFilter, new CancellationToken());

            await context.RespondAsync(new MicroserviceGetFilteredLogsResponse(
                logs,
                null));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceGetFilteredLogsResponse(
                null,
                e.Message));
        }
    }
}