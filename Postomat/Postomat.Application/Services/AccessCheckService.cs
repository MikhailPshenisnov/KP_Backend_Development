using ConsumerException = Postomat.Core.Exceptions.BaseExceptions.ConsumerException;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Exceptions.BaseExceptions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Postomat.Core.Exceptions.SpecificExceptions;
using Postomat.Core.MessageBrokerContracts.Requests;
using Postomat.Core.MessageBrokerContracts.Responses;
using Postomat.Core.Models;

namespace Postomat.Application.Services;

public class AccessCheckService : IAccessCheckService
{
    private readonly IRequestClient<MicroserviceValidateTokenRequest> _validateTokenClient;
    private readonly IUsersService _usersService;

    public AccessCheckService(IRequestClient<MicroserviceValidateTokenRequest> validateTokenClient, 
        IUsersService usersService)
    {
        _validateTokenClient = validateTokenClient;
        _usersService = usersService;
    }

    public async Task<User> CheckAccessLvl(HttpRequest request, int minAccessLvl, CancellationToken cancellationToken)
    {
        try
        {
            request.Headers.TryGetValue("Authorization", out var authHeader);
            var authToken = authHeader.ToString().Replace("Bearer ", string.Empty);

            var response = (await _validateTokenClient.GetResponse<MicroserviceValidateTokenResponse>(
                    new MicroserviceValidateTokenRequest(authToken),
                    cancellationToken))
                .Message;

            if (response.ErrorMessage is not null)
            {
                throw new ConsumerException($"Something went wrong while authorization process. " +
                                            $"--> {response.ErrorMessage}");
            }

            if (!response.IsValid)
            {
                if (response.Message is null)
                    throw new ConsumerException("If token is not valid, the message cannot be empty.");
                throw new AccessException(response.Message);
            }

            if (response.UserDto is null)
                throw new ConsumerException("If token is valid, the user cannot be empty.");

            var user = await _usersService.GetUserAsync(response.UserDto.UserId, cancellationToken);

            if (user.Role.AccessLvl <= minAccessLvl)
                return user;
            throw new AccessException("The user does not have sufficient access rights");
        }
        catch (ServiceException e)
        {
            throw new ServiceException($"Something went wrong while authorization process. " +
                                       $"--> {e.Message}");
        }
    }
}