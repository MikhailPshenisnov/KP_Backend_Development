using MassTransit;
using Microsoft.IdentityModel.Tokens;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Contracrs;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;

namespace Postomat.Application.Services;

public class ControllerErrorLogService : IControllerErrorLogService
{
    private IRequestClient<MicroserviceCreateLogRequest> _createLogClient;

    public ControllerErrorLogService(IRequestClient<MicroserviceCreateLogRequest> createLogClient)
    {
        _createLogClient = createLogClient;
    }

    public async Task<BaseResponse<T>> CreateErrorLog<T>(string origin, string title, string message)
    {
        try
        {
            var (log, error) = Log.Create(Guid.NewGuid(), DateTime.Now.ToUniversalTime(), origin, "Error",
                title, message);

            if (!error.IsNullOrEmpty())
                throw new Exception($"Unable to create error log: {error}");

            var response = (await _createLogClient.GetResponse<MicroserviceCreateLogResponse>(
                new MicroserviceCreateLogRequest(new LogDto(log.Id, log.Date, log.Origin, log.Type, log.Title,
                    log.Message)))).Message;

            if (response.ErrorMessage is not null)
                throw new Exception($"Unable to create error log (microservice error): {error}");

            return new BaseResponse<T>(
                null,
                message + $" Error log was created: \"{response.CreatedLogId}\"");
        }
        catch (Exception ex)
        {
            return new BaseResponse<T>(
                null,
                message + $" Error log was not created: \"{ex.Message}\""
            );
        }
    }
}