using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.AuthorizationMicroservice.Consumers;

public class LoginUserConsumer : IConsumer<MicroserviceLoginUserRequest>
{
    private readonly IAuthorizationService _authorizationService;

    public LoginUserConsumer(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task Consume(ConsumeContext<MicroserviceLoginUserRequest> context)
    {
        try
        {
            var request = context.Message;

            var token = await _authorizationService.LoginUser(request.Login, request.Password,
                new CancellationToken());

            await context.RespondAsync(new MicroserviceLoginUserResponse(
                true,
                token,
                "Ok",
                null));
        }
        catch (ServiceException e)
        {
            await context.RespondAsync(new MicroserviceLoginUserResponse(
                false,
                null,
                e.Message,
                null));
        }
        catch (ExpectedException e)
        {
            await context.RespondAsync(new MicroserviceLoginUserResponse(
                false,
                null,
                null,
                $"Unexpected expected error. {e.Message}"));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceLoginUserResponse(
                false,
                null,
                null,
                $"Unexpected unexpected error. {e.Message}"));
        }
    }
}