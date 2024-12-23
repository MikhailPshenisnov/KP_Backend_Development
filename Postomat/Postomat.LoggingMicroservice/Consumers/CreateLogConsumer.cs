using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Postomat.Core.Abstractions.Services;
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

            if (!error.IsNullOrEmpty())
                throw new Exception("Unable to convert log dto to log model");

            var createdLogId = await _logsService.CreateLogAsync(log, new CancellationToken());

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