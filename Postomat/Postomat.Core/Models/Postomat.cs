namespace Postomat.Core.Models;

public class Postomat
{
    public const int MaxNameLength = 128;
    public const int MaxAddressLength = 128;
    public const int MinNameLength = 8;
    public const int MinAddressLength = 20;

    private Postomat(Guid id, string name, string address, List<Cell> cells)
    {
        Id = id;
        Name = name;
        Address = address;
        Cells = cells;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Address { get; }
    public List<Cell> Cells { get; }

    private static string BasicChecks(string name, string address, List<Cell> cells)
    {
        var error = string.Empty;

        if (string.IsNullOrEmpty(name) || name.Length > MaxNameLength)
        {
            error = $"Name can't be longer than {MaxNameLength} characters or empty.";
        }
        else if (name.Length < MinNameLength)
        {
            error = $"Name can't be shorter than {MinNameLength} characters.";
        }
        else if (string.IsNullOrEmpty(address) || address.Length > MaxAddressLength)
        {
            error = $"Address can't be longer than {MaxAddressLength} characters or empty.";
        }
        else if (address.Length < MinAddressLength)
        {
            error = $"Address can't be shorter than {MinAddressLength} characters.";
        }

        return error;
    }

    public static (Postomat Postomat, string Error) Create(Guid id, string name, string address, List<Cell> cells)
    {
        var error = BasicChecks(name, address, cells);

        var postomat = new Postomat(id, name, address, cells);

        return (postomat, error);
    }
}