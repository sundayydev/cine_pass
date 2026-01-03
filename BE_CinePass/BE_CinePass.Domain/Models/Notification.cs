using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NotificationActionType = BE_CinePass.Shared.Common.NotificationActionType;
using NotificationPriority = BE_CinePass.Shared.Common.NotificationPriority;
using NotificationType = BE_CinePass.Shared.Common.NotificationType;

namespace BE_CinePass.Domain.Models;

[Table("notifications")]
public class Notification
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Required]
    [Column("type")]
    public NotificationType Type { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("message", TypeName = "text")]
    public string Message { get; set; } = string.Empty;

    [Column("data", TypeName = "jsonb")]
    public string? Data { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [MaxLength(500)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("action_type")]
    public NotificationActionType? ActionType { get; set; }

    [Column("action_data", TypeName = "text")]
    public string? ActionData { get; set; }

    [Required]
    [Column("priority")]
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
}
