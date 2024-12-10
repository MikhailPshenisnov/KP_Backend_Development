﻿namespace Postomat.Core.Models;

public class User
{
    public const int MaxLoginLength = 128;
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
            error = $"Login can't be longer than {MaxLoginLength} characters or empty";
        }
        else if (string.IsNullOrEmpty(passwordHash) || passwordHash.Length > MaxPasswordHashLength)
        {
            error = $"Password hash can't be longer than {MaxPasswordHashLength} characters or empty";
        }

        return error;
    }

    public static (User User, string Error) Create(Guid id, string login, string passwordHash, Role role)
    {
        var error = BasicChecks(login, passwordHash);

        var user = new User(id, login, passwordHash, role);

        return (user, error);
    }
}