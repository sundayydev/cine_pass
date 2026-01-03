using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.DeviceToken;

public class SendTestPushDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Body { get; set; } = string.Empty;

    public Dictionary<string, string>? Data { get; set; }

    public string? ImageUrl { get; set; }
}