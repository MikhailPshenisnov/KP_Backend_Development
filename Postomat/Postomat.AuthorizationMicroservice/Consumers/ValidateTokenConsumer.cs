﻿using MassTransit;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.MessageBrokerContracts;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;

namespace Postomat.AuthorizationMicroservice.Consumers;

public class ValidateTokenConsumer : IConsumer<MicroserviceValidateTokenRequest>
{
    private readonly IAuthorizationService _authorizationService;

    public ValidateTokenConsumer(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task Consume(ConsumeContext<MicroserviceValidateTokenRequest> context)
    {
        try
        {
            var request = context.Message;

            var (userId, roleId) = await _authorizationService.ValidateToken(request.Token,
                new CancellationToken());

            await context.RespondAsync(new MicroserviceValidateTokenResponse(
                true,
                new UserDto(userId, roleId),
                null));
        }
        catch (Exception e)
        {
            await context.RespondAsync(new MicroserviceValidateTokenResponse(
                false,
                null,
                e.Message));
        }
    }
}