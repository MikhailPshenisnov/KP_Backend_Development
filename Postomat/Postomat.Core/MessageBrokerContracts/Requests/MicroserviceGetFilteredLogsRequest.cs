﻿namespace Postomat.Core.MessageBrokerContracts.Requests;

public record MicroserviceGetFilteredLogsRequest(
    LogFilterDto? LogFilterDto
);