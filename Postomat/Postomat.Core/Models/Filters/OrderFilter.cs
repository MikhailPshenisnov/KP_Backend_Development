using Postomat.Core.Models.Other;

namespace Postomat.Core.Models.Filters;

public class OrderFilter
{
    private OrderFilter(SizeEnumerator? orderSizeFrom, SizeEnumerator? orderSizeTo)
    {
        OrderSizeFrom = orderSizeFrom;
        OrderSizeTo = orderSizeTo;
    }

    public SizeEnumerator? OrderSizeFrom { get; }
    public SizeEnumerator? OrderSizeTo { get; }

    private static string BasicChecks(SizeEnumerator? orderSizeFrom, SizeEnumerator? orderSizeTo)
    {
        var error = string.Empty;

        if (orderSizeFrom is not null &&
            (orderSizeFrom < SizeEnumerator.Small || orderSizeFrom > SizeEnumerator.Large))
        {
            error = $"Size can be between {(int)SizeEnumerator.Small} and {(int)SizeEnumerator.Large}";
        }
        else if (orderSizeTo is not null &&
                 (orderSizeTo < SizeEnumerator.Small || orderSizeTo > SizeEnumerator.Large))
        {
            error = $"Size can be between {(int)SizeEnumerator.Small} and {(int)SizeEnumerator.Large}";
        }
        else if (orderSizeFrom is not null && orderSizeTo is not null && orderSizeFrom > orderSizeTo)
        {
            error = "Wrong order of order size from and order size to";
        }

        return error;
    }

    public static (OrderFilter OrderFilter, string Error) Create(SizeEnumerator? orderSizeFrom,
        SizeEnumerator? orderSizeTo)
    {
        var error = BasicChecks(orderSizeFrom, orderSizeTo);

        var orderFilter = new OrderFilter(orderSizeFrom, orderSizeTo);

        return (orderFilter, error);
    }
}