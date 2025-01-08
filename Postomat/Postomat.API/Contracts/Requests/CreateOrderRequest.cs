using Postomat.Core.Models.Other;

namespace Postomat.API.Contracts.Requests;

public record CreateOrderRequest(
    string ReceivingCode,
    SizeEnumerator OrderSize
);