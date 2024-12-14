using Postomat.Core.Models.Other;

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

        if (accessLvlFrom is not null &&
            (accessLvlFrom < MinAccessLvl || accessLvlFrom > MaxAccessLvl))
        {
            error = $"Access lvl can be between {MinAccessLvl} and {MaxAccessLvl}";
        }
        else if (accessLvlTo is not null &&
                 (accessLvlTo < MinAccessLvl || accessLvlTo > MaxAccessLvl))
        {
            error = $"Access lvl can be between {MinAccessLvl} and {MaxAccessLvl}";
        }
        else if (accessLvlFrom is not null && accessLvlTo is not null && accessLvlFrom > accessLvlTo)
        {
            error = "Wrong order of access level from and access level to";
        }

        return error;
    }

    public static (RoleFilter RoleFilter, string Error) Create(string? partOfRoleName, int? accessLvlFrom,
        int? accessLvlTo)
    {
        var error = BasicChecks(accessLvlFrom, accessLvlTo);

        var roleFilter = new RoleFilter(partOfRoleName, accessLvlFrom, accessLvlTo);

        return (roleFilter, error);
    }
}