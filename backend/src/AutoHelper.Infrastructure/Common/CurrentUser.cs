using System.Security.Claims;
using AutoHelper.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AutoHelper.Infrastructure.Common;

/// <summary>
/// Reads the current user identity from the HTTP context JWT claims.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? Id
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin =>
        User?.IsInRole("admin") == true || User?.IsInRole("superadmin") == true;
}
