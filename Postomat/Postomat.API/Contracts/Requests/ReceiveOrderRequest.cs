namespace Postomat.API.Contracts.Requests;

public record ReceiveOrderRequest(
    Guid PostomatId,
    string ReceivingCode
);