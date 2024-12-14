namespace Postomat.Core.Models.Filters;

public class PostomatFilter
{
    private PostomatFilter(string? partOfName, string? partOfAddress)
    {
        PartOfName = partOfName;
        PartOfAddress = partOfAddress;
    }

    public string? PartOfName { get; }
    public string? PartOfAddress { get; }

    private static string BasicChecks()
    {
        var error = string.Empty;

        return error;
    }

    public static (PostomatFilter PostomatFilter, string Error) Create(string? partOfName, string? partOfAddress)
    {
        var error = BasicChecks();

        var postomatFilter = new PostomatFilter(partOfName, partOfAddress);

        return (postomatFilter, error);
    }
}