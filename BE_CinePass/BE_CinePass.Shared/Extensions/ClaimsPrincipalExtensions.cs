using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BE_CinePass.Shared.Common;

namespace BE_CinePass.Shared.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to easily extract user information from JWT
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Get user ID from JWT claims
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }

    /// <summary>
    /// Get user email from JWT claims
    /// </summary>
    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }

    /// <summary>
    /// Get user role from JWT claims
    /// </summary>
    public static UserRole? GetUserRole(this ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value
                        ?? principal.FindFirst("role")?.Value;

        if (string.IsNullOrEmpty(roleClaim))
            return null;

        if (Enum.TryParse<UserRole>(roleClaim, true, out var role))
            return role;

        return null;
    }

    /// <summary>
    /// Get user full name from JWT claims
    /// </summary>
    public static string? GetUserFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value
               ?? principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
    }

    /// <summary>
    /// Check if user is in specific role
    /// </summary>
    public static bool IsInRole(this ClaimsPrincipal principal, UserRole role)
    {
        var userRole = principal.GetUserRole();
        return userRole.HasValue && userRole.Value == role;
    }

    /// <summary>
    /// Check if user is Admin
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(UserRole.Admin);
    }

    /// <summary>
    /// Check if user is Staff
    /// </summary>
    public static bool IsStaff(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(UserRole.Staff);
    }

    /// <summary>
    /// Check if user is Customer
    /// </summary>
    public static bool IsCustomer(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(UserRole.Customer);
    }

    /// <summary>
    /// Check if user is Admin or Staff
    /// </summary>
    public static bool IsAdminOrStaff(this ClaimsPrincipal principal)
    {
        return principal.IsAdmin() || principal.IsStaff();
    }
}
