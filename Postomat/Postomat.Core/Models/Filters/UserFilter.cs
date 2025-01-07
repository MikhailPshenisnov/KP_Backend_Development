namespace Postomat.Core.Models.Filters;

public class UserFilter
{
    private UserFilter(string? partOfLogin, Guid? roleId)
    {
        PartOfLogin = partOfLogin;
        RoleId = roleId;
    }

    public string? PartOfLogin { get; }
    public Guid? RoleId { get; }

    private static string BasicChecks()
    {
        var error = string.Empty;

        return error;
    }

    public static (UserFilter UserFilter, string Error) Create(string? partOfLogin, Guid? roleId)
    {
        if (partOfLogin == string.Empty)
            partOfLogin = null;

        var error = BasicChecks();

        var userFilter = new UserFilter(partOfLogin, roleId);

        return (userFilter, error);
    }
}