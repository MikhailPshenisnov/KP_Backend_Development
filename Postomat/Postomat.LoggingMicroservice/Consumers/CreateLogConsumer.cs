using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;

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

            var (log, error) = Log.Create(request.LogDto.Id, request.LogDto.Date, request.LogDto.Origin,
                request.LogDto.Type, request.LogDto.Title, request.LogDto.Message);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert log dto to log model. " +
                                              $"--> {error}");

            var createdLogId = await _logsService.CreateLogAsync(log, new CancellationToken());

            await context.RespondAsync(new MicroserviceCreateLogResponse(
                createdLogId,
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            await context.RespondAsync(new MicroserviceCreateLogResponse(
                null,
                e.Message));
        }
        catch (ExpectedException e)
        {
            await context.RespondAsync(new MicroserviceCreateLogResponse(
                null,
                $"Unexpected expected error. {e.Message}"));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceCreateLogResponse(
                null,
                $"Unexpected unexpected error. {e.Message}"));
        }
    }
}