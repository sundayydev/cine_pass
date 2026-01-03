using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("notification_settings")]
public class NotificationSettings
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("enable_upcoming_showtime")]
    public bool EnableUpcomingShowtime { get; set; } = true;

    [Column("enable_order_status")]
    public bool EnableOrderStatus { get; set; } = true;

    [Column("enable_promotion")]
    public bool EnablePromotion { get; set; } = true;

    [Column("enable_system")]
    public bool EnableSystem { get; set; } = true;
    
    [Column("push_enable")]
    public bool PushEnabled { get; set; } = true;
    [Column("showtime_reminder_minutes")]
    public int ShowtimeReminderMinutes { get; set; } = 120;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User? User { get; set; }
}
