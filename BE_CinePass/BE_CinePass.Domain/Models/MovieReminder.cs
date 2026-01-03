using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

/// <summary>
/// Lưu thông tin user đăng ký nhận thông báo khi phim ra mắt
/// </summary>
[Table("movie_reminders")]
public class MovieReminder
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("movie_id")]
    public Guid MovieId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("notified_at")]
    public DateTime? NotifiedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(MovieId))]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}