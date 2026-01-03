using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.DeviceToken;

public class DeviceTokenRegisterDto
{
    [Required(ErrorMessage = "Token is required")]
    [MaxLength(500, ErrorMessage = "Token cannot exceed 500 characters")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Platform is required")]
    [RegularExpression("^(ios|android|web)$", ErrorMessage = "Platform must be ios, android, or web")]
    public string Platform { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Device model cannot exceed 100 characters")]
    public string? DeviceModel { get; set; }

    [MaxLength(20, ErrorMessage = "App version cannot exceed 20 characters")]
    public string? AppVersion { get; set; }
}