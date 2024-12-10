namespace Postomat.Core.Models;

public class Log
{
    public const int MaxOriginLength = 128;
    public const int MaxTypeLength = 128;
    public const int MaxTitleLength = 128;

    private Log(Guid id, DateTime date, string origin, string type, string title, string message)
    {
        Id = id;
        Date = date;
        Origin = origin;
        Type = type;
        Title = title;
        Message = message;
    }

    public Guid Id { get; }
    public DateTime Date { get; }
    public string Origin { get; }
    public string Type { get; }
    public string Title { get; }
    public string Message { get; }

    private static string BasicChecks(DateTime date, string origin, string type, string title)
    {
        var error = string.Empty;

        if (date > DateTime.Now)
        {
            error = "Date can't be from the future";
        }
        else if (string.IsNullOrEmpty(origin) || origin.Length > MaxOriginLength)
        {
            error = $"Origin can't be longer than {MaxOriginLength} characters or empty";
        }
        else if (string.IsNullOrEmpty(type) || type.Length > MaxTypeLength)
        {
            error = $"Type can't be longer than {MaxTypeLength} characters or empty";
        }
        else if (string.IsNullOrEmpty(title) || title.Length > MaxTitleLength)
        {
            error = $"Title can't be longer than {MaxTitleLength} characters or empty";
        }

        return error;
    }

    public static (Log Log, string Error) Create(Guid id, DateTime date, string origin, string type, string title,
        string message)
    {
        var error = BasicChecks(date, origin, type, title);

        var universalDate = date.ToUniversalTime();

        var log = new Log(id, date, origin, type, title, message);

        return (log, error);
    }
}