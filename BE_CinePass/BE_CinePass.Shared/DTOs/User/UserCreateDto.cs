using BE_CinePass.Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.User;

public class UserCreateDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? FullName { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;
}

