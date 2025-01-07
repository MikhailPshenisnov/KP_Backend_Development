using System.Net;
using System.Text.Json;
using MassTransit;
using Postomat.Core.Contracrs;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.Exceptions.SpecificExceptions.ControllerExceptions;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;

namespace Postomat.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ExpectedException e) when (e is AccessException or ReceivingException or DeliveringException)
        {
            await HandleExceptionWithoutLogAsync(context, e);
        }
        catch (ExpectedException e) when (e is ControllerException or Core.Exceptions.BaseExceptions.ConsumerException)
        {
            await HandleExceptionAsync(context, new ExpectedException("Expected error.", e));
        }
        catch (ExpectedException e)
        {
            await HandleExceptionAsync(context, new ExpectedException("Unexpected expected error.", e));
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, new ExpectedException("Unexpected unexpected error.", e));
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, ExpectedException exception)
    {
        try
        {
            using var scope = context.RequestServices.CreateScope();
            var createLogClient = scope.ServiceProvider
                .GetRequiredService<IRequestClient<MicroserviceCreateLogRequest>>();
            
            var (log, error) = Log.Create(
                id: Guid.NewGuid(),
                date: DateTime.Now.ToUniversalTime(),
                origin: exception.InnerException?.StackTrace?.Split("\r\n")[0].TrimStart()[3..].Split(" in ")[0] ??
                        exception.StackTrace?.Split("\r\n")[0].TrimStart()[3..].Split(" in ")[0] ?? string.Empty,
                type: exception.InnerException?.GetType().ToString() ??
                      exception.GetType().ToString(),
                title: exception.InnerException is not null
                    ? !string.IsNullOrEmpty(exception.Message)
                        ? exception.Message.Replace(".", string.Empty)
                        : exception.InnerException.GetType().Name
                    : exception.GetType().Name,
                message: exception.InnerException is not null
                    ? exception.InnerException.Message
                    : exception.Message);

            if (!string.IsNullOrEmpty(error))
                throw new ConversionException($"Unable to create error log. " +
                                              $"--> {error}");

            var microserviceResponse = (await createLogClient
                .GetResponse<MicroserviceCreateLogResponse>(new MicroserviceCreateLogRequest(
                    new LogDto(log.Id, log.Date, log.Origin, log.Type, log.Title, log.Message)))).Message;

            if (microserviceResponse.ErrorMessage is not null)
                throw new Core.Exceptions.BaseExceptions.ConsumerException(
                    $"Unable to create error log (microservice error). " +
                    $"--> {error}");

            var errorResponse = new BaseResponse<string>(
                null,
                $"{log.Message} Error log \"{microserviceResponse.CreatedLogId}\" was created.");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
        catch (Exception e)
        {
            var message = exception.InnerException is not null
                ? exception.InnerException.Message
                : exception.Message;

            var errorResponse = new BaseResponse<string>(
                null,
                $"{message} Error log was not created. " +
                $"--> {e.Message}");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }

    private async Task HandleExceptionWithoutLogAsync(HttpContext context, ExpectedException exception)
    {
        var errorResponse = new BaseResponse<string>(
            null,
            exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}