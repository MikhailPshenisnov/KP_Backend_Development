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
        if (partOfName == string.Empty)
            partOfName = null;
        if (partOfAddress == string.Empty)
            partOfAddress = null;

        var error = BasicChecks();

        var postomatFilter = new PostomatFilter(partOfName, partOfAddress);

        return (postomatFilter, error);
    }
}