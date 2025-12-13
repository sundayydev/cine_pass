using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.User;

public class UserUpdateDto
{
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? FullName { get; set; }

    [MinLength(6)]
    public string? Password { get; set; }
}

