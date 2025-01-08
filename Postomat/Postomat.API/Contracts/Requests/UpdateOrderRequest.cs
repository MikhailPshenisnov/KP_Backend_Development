using Postomat.Core.Models.Other;

namespace Postomat.API.Contracts.Requests;

public record UpdateOrderRequest(
    Guid OrderId,
    string NewOrderReceivingCode,
    SizeEnumerator NewOrderOrderSize
);