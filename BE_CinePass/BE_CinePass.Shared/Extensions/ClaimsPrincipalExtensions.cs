using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace BE_CinePass.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return null;

        var userId =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(userId, out var id) ? id : null;
    }

    public static string? GetUserEmail(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return null;

        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }

    public static string? GetUserRole(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return null;

        return principal.FindFirst(ClaimTypes.Role)?.Value
            ?? principal.FindFirst("role")?.Value;
    }

    public static string? GetUserFullName(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return null;

        return principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
    }
}
