using System.Diagnostics.CodeAnalysis;

namespace Postomat.DataAccess.Database.Entities;

public class Log
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Origin { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;

    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public string Message { get; set; } = null!;
}