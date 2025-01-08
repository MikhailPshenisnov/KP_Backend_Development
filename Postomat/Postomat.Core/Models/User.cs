namespace Postomat.Core.Models;

public class User
{
    public const int MaxLoginLength = 128;
    public const int MinLoginLength = 5;
    public const int MaxPasswordHashLength = 128;

    private User(Guid id, string login, string passwordHash, Role role)
    {
        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    public Guid Id { get; }
    public string Login { get; }
    public string PasswordHash { get; }
    public Role Role { get; }

    private static string BasicChecks(string login, string passwordHash)
    {
        var error = string.Empty;

        if (string.IsNullOrEmpty(login) || login.Length > MaxLoginLength)
        {
            error = $"Login can't be longer than {MaxLoginLength} characters or empty.";
        }
        else if (login.Length < MinLoginLength)
        {
            error = $"Login can't be shorter than {MinLoginLength} characters.";
        }
        else if (string.IsNullOrWhiteSpace(login))
        {
            error = "Login can't be line only with whitespaces.";
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(login, "^[a-zA-Z0-9_.-]+$"))
        {
            error = "Incorrect symbols in login.";
        }
        else if (string.IsNullOrEmpty(passwordHash) || passwordHash.Length > MaxPasswordHashLength)
        {
            error = $"Password hash can't be longer than {MaxPasswordHashLength} characters or empty.";
        }

        return error;
    }

    public static (User User, string Error) Create(Guid id, string login, string passwordHash, Role role)
    {
        var error = BasicChecks(login, passwordHash);

        var user = new User(id, login, passwordHash, role);

        return (user, error);
    }

    public static string PasswordCheck(string password)
    {
        var passwordCheckError = string.Empty;
        
        if (string.IsNullOrWhiteSpace(password))
        {
            passwordCheckError = "The password cannot be empty.";
        }
        else if (password.Length is < 8 or > 25)
        {
            passwordCheckError = "The password must be between 8 and 25 characters.";
        }
        else if (!password.Any(char.IsDigit))
        {
            passwordCheckError = "The password must contain at least one number.";
        }
        else if (!password.Any(char.IsUpper))
        {
            passwordCheckError = "The password must contain at least one capital letter.";
        }
        else if (!password.Any(char.IsLower))
        {
            passwordCheckError = "The password must contain at least one lowercase letter.";
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
        {
            passwordCheckError = "The password must contain at least one special character.";
        }
        
        return passwordCheckError;
    }
}