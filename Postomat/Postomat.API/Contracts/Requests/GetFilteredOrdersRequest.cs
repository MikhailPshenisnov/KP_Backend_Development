using Postomat.Core.Models.Other;

namespace Postomat.API.Contracts.Requests;

public record GetFilteredOrdersRequest(
    SizeEnumerator? OrderSizeFrom,
    SizeEnumerator? OrderSizeTo
);