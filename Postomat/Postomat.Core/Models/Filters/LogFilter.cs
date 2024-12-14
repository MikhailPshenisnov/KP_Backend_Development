namespace Postomat.Core.Models.Filters;

public class LogFilter
{
    private LogFilter(DateTime? dateFrom, DateTime? dateTo, string? partOfOrigin, string? partOfType,
        string? partOfTitle, string? partOfMessage)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
        PartOfOrigin = partOfOrigin;
        PartOfType = partOfType;
        PartOfTitle = partOfTitle;
        PartOfMessage = partOfMessage;
    }

    public DateTime? DateFrom { get; }
    public DateTime? DateTo { get; }
    public string? PartOfOrigin { get; }
    public string? PartOfType { get; }
    public string? PartOfTitle { get; }
    public string? PartOfMessage { get; }

    private static string BasicChecks(DateTime? dateFrom, DateTime? dateTo)
    {
        var error = string.Empty;

        if (dateFrom is not null && dateTo > DateTime.Now)
        {
            error = "Date from can't be from the future";
        }
        else if (dateFrom is not null && dateTo is not null && dateFrom > dateTo)
        {
            error = "Wrong order of date from and date to";
        }

        return error;
    }

    public static (LogFilter LogFilter, string Error) Create(DateTime? dateFrom, DateTime? dateTo, string? partOfOrigin,
        string? partOfType, string? partOfTitle, string? partOfMessage)
    {
        var error = BasicChecks(dateFrom, dateTo);

        var logFilter = new LogFilter(dateFrom, dateTo, partOfOrigin, partOfType, partOfTitle, partOfMessage);

        return (logFilter, error);
    }
}