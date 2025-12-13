using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.User;

public class UserLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

