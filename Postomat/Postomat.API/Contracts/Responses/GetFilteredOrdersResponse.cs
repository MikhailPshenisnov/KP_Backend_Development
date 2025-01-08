using Postomat.Core.Models.Other;

namespace Postomat.API.Contracts.Responses;

public record GetFilteredOrdersResponse(
    Guid Id,
    string ReceivingCodeHash,
    SizeEnumerator OrderSize
);