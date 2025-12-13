using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.User;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

