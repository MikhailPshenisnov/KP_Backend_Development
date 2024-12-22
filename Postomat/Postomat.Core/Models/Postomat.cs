namespace Postomat.Core.Models;

public class Postomat
{
    public const int MaxNameLength = 128;
    public const int MaxAddressLength = 128;

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

        if (cells.Count == 0)
        {
            error = "Postomat must have at least one cell";
        }
        else if (string.IsNullOrEmpty(name) || name.Length > MaxNameLength)
        {
            error = $"Name can't be longer than {MaxNameLength} characters or empty";
        }
        else if (string.IsNullOrEmpty(address) || address.Length > MaxAddressLength)
        {
            error = $"Address can't be longer than {MaxAddressLength} characters or empty";
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