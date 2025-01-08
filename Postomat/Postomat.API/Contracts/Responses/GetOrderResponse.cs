using Postomat.Core.Models.Other;

namespace Postomat.API.Contracts.Responses;

public record GetOrderResponse(
    Guid Id,
    string ReceivingCodeHash,
    SizeEnumerator OrderSize
);