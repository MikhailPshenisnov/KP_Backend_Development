using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models.Filters;

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

            var (logFilter, error) = request.LogFilterDto is not null
                ? LogFilter.Create(request.LogFilterDto.DateFrom, request.LogFilterDto.DateTo,
                    request.LogFilterDto.PartOfOrigin, request.LogFilterDto.PartOfType,
                    request.LogFilterDto.PartOfTitle,
                    request.LogFilterDto.PartOfMessage)
                : (null, string.Empty);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to convert log filter dto to log filter model. " +
                                              $"--> {error}");

            var logs = await _logsService.GetFilteredLogsAsync(logFilter, new CancellationToken());

            await context.RespondAsync(new MicroserviceGetFilteredLogsResponse(
                logs.Select(log => new LogDto(log.Id, log.Date, log.Origin, log.Type, log.Title, log.Message)).ToList(),
                null));
        }
        catch (ExpectedException e) when (e is ConversionException or ServiceException)
        {
            await context.RespondAsync(new MicroserviceGetFilteredLogsResponse(
                null,
                e.Message));
        }
        catch (ExpectedException e)
        {
            await context.RespondAsync(new MicroserviceGetFilteredLogsResponse(
                null,
                $"Unexpected expected error. {e.Message}"));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceGetFilteredLogsResponse(
                null,
                $"Unexpected unexpected error. {e.Message}"));
        }
    }
}