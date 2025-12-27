using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("member_points")]
public class MemberPoint
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("points")]
    public int Points { get; set; } = 0; // Điểm hiện tại có thể sử dụng

    [Column("lifetime_points")]
    public int LifetimePoints { get; set; } = 0; // Tổng điểm tích lũy (dùng để xác định tier)

    [Column("tier", TypeName = "text")]
    public MemberTier Tier { get; set; } = MemberTier.Bronze; // Cấp bậc hiện tại

    [Column("points_to_expire")]
    public int PointsToExpire { get; set; } = 0; // Số điểm sắp hết hạn

    [Column("next_expiry_date")]
    public DateTime? NextExpiryDate { get; set; } // Ngày hết hạn kế tiếp

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
