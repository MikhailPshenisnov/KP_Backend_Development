using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;

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

            var (newLog, error) = Log.Create(request.NewLogDto.Id, request.NewLogDto.Date,
                request.NewLogDto.Origin, request.NewLogDto.Type, request.NewLogDto.Title, request.NewLogDto.Message);

            if (!error.IsNullOrEmpty())
                throw new Exception("Unable to convert log dto to log model");

            var updatedLogId = await _logsService.UpdateLogAsync(request.LogId, newLog,
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