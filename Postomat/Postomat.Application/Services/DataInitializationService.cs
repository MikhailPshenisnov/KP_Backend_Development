using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Postomat.Core.Abstractions.Services;
using Postomat.Core.Models;
using Postomat.Core.Models.Other;
using Postomat.DataAccess.Database.Context;

namespace Postomat.Application.Services;

public class DataInitializationService : IDataInitializationService
{
    private readonly PostomatDbContext _context;
    private readonly IConfiguration _configuration;

    public DataInitializationService(PostomatDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task InitializeData(CancellationToken cancellationToken)
    {
        var dataInitializationConfig = _configuration.GetSection("DataInitialization");

        var existedSuperuserRoleEntity = await _context.Roles
            .FirstOrDefaultAsync(r => r.AccessLvl == (int)AccessLvlEnumerator.SuperUser, cancellationToken);
        if (existedSuperuserRoleEntity is null)
        {
            var superuserRole = Role.Create(
                Guid.NewGuid(),
                dataInitializationConfig["DefaultSuperuserRoleName"] ?? string.Empty,
                (int)AccessLvlEnumerator.SuperUser);
            if (!string.IsNullOrEmpty(superuserRole.Error))
                throw new Exception($"Unable to create superuser role. " +
                                    $"--> {superuserRole.Error}");

            var superuserRoleEntity = new DataAccess.Database.Entities.Role
            {
                Id = superuserRole.Role.Id,
                RoleName = superuserRole.Role.RoleName,
                AccessLvl = superuserRole.Role.AccessLvl
            };

            await _context.Roles.AddAsync(superuserRoleEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var existedBaseUserRoleEntity = await _context.Roles
            .FirstOrDefaultAsync(r => r.AccessLvl == (int)AccessLvlEnumerator.FiredEmployee, cancellationToken);
        if (existedBaseUserRoleEntity is null)
        {
            var baseUserRole = Role.Create(
                Guid.NewGuid(),
                dataInitializationConfig["DefaultBaseUserRoleName"] ?? string.Empty,
                (int)AccessLvlEnumerator.FiredEmployee);
            if (!string.IsNullOrEmpty(baseUserRole.Error))
                throw new Exception($"Unable to create base user role. " +
                                    $"--> {baseUserRole.Error}");

            var baseUserRoleEntity = new DataAccess.Database.Entities.Role
            {
                Id = baseUserRole.Role.Id,
                RoleName = baseUserRole.Role.RoleName,
                AccessLvl = baseUserRole.Role.AccessLvl
            };

            await _context.Roles.AddAsync(baseUserRoleEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var userEntities = await _context.Users.ToListAsync(cancellationToken);
        var suRoleEntity = (await _context.Roles
            .FirstOrDefaultAsync(r => r.AccessLvl == (int)AccessLvlEnumerator.SuperUser, cancellationToken))!;
        var suRole = Role.Create(
            suRoleEntity.Id,
            suRoleEntity.RoleName,
            suRoleEntity.AccessLvl);
        if (!string.IsNullOrEmpty(suRole.Error))
            throw new Exception($"Unable to create super user. " +
                                $"--> {suRole.Error}");
        var existedSuperuserEntity = userEntities
            .FirstOrDefault(u => u.RoleId == suRole.Role.Id);
        if (existedSuperuserEntity is null)
        {
            var password = dataInitializationConfig["DefaultSuperuserPassword"] ?? string.Empty;

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

            if (!string.IsNullOrEmpty(passwordCheckError))
                throw new Exception($"Unable to create super user. " +
                                    $"--> {passwordCheckError}");

            var su = User.Create(
                Guid.NewGuid(),
                dataInitializationConfig["DefaultSuperuserLogin"] ?? string.Empty,
                BCrypt.Net.BCrypt.EnhancedHashPassword(
                    dataInitializationConfig["DefaultSuperuserPassword"] ?? string.Empty),
                suRole.Role);
            if (!string.IsNullOrEmpty(su.Error))
                throw new Exception($"Unable to create super user. " +
                                    $"--> {su.Error}");

            var suEntity = new DataAccess.Database.Entities.User
            {
                Id = su.User.Id,
                Login = su.User.Login,
                PasswordHash = su.User.PasswordHash,
                RoleId = su.User.Role.Id
            };

            await _context.Users.AddAsync(suEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}