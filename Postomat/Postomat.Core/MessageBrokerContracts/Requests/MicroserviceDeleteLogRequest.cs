﻿namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceDeleteLogRequest(
    Guid LogId
);