namespace Postomat.Core.Models;

public class Role
{
    public const int MaxRoleNameLength = 128;
    public const int MinAccessLvl = 0;
    public const int MaxAccessLvl = 999;

    private Role(Guid id, string roleName, int accessLvl)
    {
        Id = id;
        RoleName = roleName;
        AccessLvl = accessLvl;
    }

    public Guid Id { get; }
    public string RoleName { get; }
    public int AccessLvl { get; }

    private static string BasicChecks(string roleName, int accessLvl)
    {
        var error = string.Empty;

        if (string.IsNullOrEmpty(roleName) || roleName.Length > MaxRoleNameLength)
        {
            error = $"Role name can't be longer than {MaxRoleNameLength} characters or empty";
        }
        else if (accessLvl < MinAccessLvl || accessLvl > MaxAccessLvl)
        {
            error = $"Access level must be between {MinAccessLvl} and {MaxAccessLvl}";
        }

        return error;
    }

    public static (Role Role, string Error) Create(Guid id, string roleName, int accessLvl)
    {
        var error = BasicChecks(roleName, accessLvl);

        var order = new Role(id, roleName, accessLvl);

        return (order, error);
    }
}