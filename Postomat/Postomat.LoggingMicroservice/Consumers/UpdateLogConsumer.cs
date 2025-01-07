using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
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

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert log dto to log model. " +
                                              $"--> {error}");

            var updatedLogId = await _logsService.UpdateLogAsync(request.LogId, newLog,
                new CancellationToken());

            await context.RespondAsync(new MicroserviceUpdateLogResponse(
                updatedLogId,
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            await context.RespondAsync(new MicroserviceUpdateLogResponse(
                null,
                e.Message));
        }
        catch (ExpectedException e)
        {
            await context.RespondAsync(new MicroserviceUpdateLogResponse(
                null,
                $"Unexpected expected error. {e.Message}"));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceUpdateLogResponse(
                null,
                $"Unexpected unexpected error. {e.Message}"));
        }
    }
}