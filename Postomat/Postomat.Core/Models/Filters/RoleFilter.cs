namespace Postomat.Core.Models.Filters;

public class RoleFilter
{
    public const int MinAccessLvl = Role.MinAccessLvl;
    public const int MaxAccessLvl = Role.MaxAccessLvl;


    private RoleFilter(string? partOfRoleName, int? accessLvlFrom, int? accessLvlTo)
    {
        PartOfRoleName = partOfRoleName;
        AccessLvlFrom = accessLvlFrom;
        AccessLvlTo = accessLvlTo;
    }

    public string? PartOfRoleName { get; }
    public int? AccessLvlFrom { get; }
    public int? AccessLvlTo { get; }

    private static string BasicChecks(int? accessLvlFrom, int? accessLvlTo)
    {
        var error = string.Empty;

        if (accessLvlFrom is < MinAccessLvl or > MaxAccessLvl)
        {
            error = $"Access lvl can be between {MinAccessLvl} and {MaxAccessLvl}.";
        }
        else if (accessLvlTo is < MinAccessLvl or > MaxAccessLvl)
        {
            error = $"Access lvl can be between {MinAccessLvl} and {MaxAccessLvl}.";
        }
        else if (accessLvlFrom > accessLvlTo)
        {
            error = "Wrong order of access level from and access level to.";
        }

        return error;
    }

    public static (RoleFilter RoleFilter, string Error) Create(string? partOfRoleName, int? accessLvlFrom,
        int? accessLvlTo)
    {
        if (partOfRoleName == string.Empty)
            partOfRoleName = null;

        var error = BasicChecks(accessLvlFrom, accessLvlTo);

        var roleFilter = new RoleFilter(partOfRoleName, accessLvlFrom, accessLvlTo);

        return (roleFilter, error);
    }
}