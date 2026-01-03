using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.DeviceToken;

public class BroadcastPushDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Body { get; set; } = string.Empty;

    public Dictionary<string, string>? Data { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>
    /// Optional: Limit to specific platform (ios, android, web)
    /// </summary>
    public string? Platform { get; set; }
}