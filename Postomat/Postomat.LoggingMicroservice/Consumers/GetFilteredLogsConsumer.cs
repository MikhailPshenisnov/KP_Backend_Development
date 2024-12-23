using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Postomat.Core.Abstractions.Services;
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
                : (null, null);

            if (!error.IsNullOrEmpty())
                throw new Exception("Unable to convert log filter dto to log filter model");

            var logs = await _logsService.GetFilteredLogsAsync(logFilter, new CancellationToken());

            await context.RespondAsync(new MicroserviceGetFilteredLogsResponse(
                logs.Select(log => new LogDto(log.Id, log.Date, log.Origin, log.Type, log.Title, log.Message)).ToList(),
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