using Microsoft.AspNetCore.Http;
using Postomat.Core.Models;

namespace Postomat.Core.Abstractions.Services;

public interface IAccessCheckService
{
    Task<User> CheckAccessLvl(HttpRequest request, int minAccessLvl, CancellationToken ct);
}