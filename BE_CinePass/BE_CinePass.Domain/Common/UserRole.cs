namespace BE_CinePass.Domain.Common;

/// <summary>
/// User role enumeration
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Customer role - Default user role
    /// </summary>
    Customer = 0,

    /// <summary>
    /// Staff role - Cinema staff with limited admin access
    /// </summary>
    Staff = 1,

    /// <summary>
    /// Admin role - Full system access
    /// </summary>
    Admin = 2
}
