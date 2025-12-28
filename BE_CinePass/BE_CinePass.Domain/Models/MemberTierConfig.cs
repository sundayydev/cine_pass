using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

/// <summary>
/// Cấu hình cho các cấp bậc hội viên
/// Định nghĩa ngưỡng điểm và quyền lợi cho từng cấp
/// </summary>
[Table("member_tier_configs")]
public class MemberTierConfig
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tier", TypeName = "text")]
    public MemberTier Tier { get; set; }

    [Required]
    [Column("name", TypeName = "text")]
    public string Name { get; set; } = string.Empty; // Tên hiển thị: Đồng, Bạc, Vàng, Kim Cương

    [Required]
    [Column("min_points")]
    public int MinPoints { get; set; } // Số điểm tối thiểu để đạt cấp này

    [Column("max_points")]
    public int? MaxPoints { get; set; } // Số điểm tối đa (null cho cấp cao nhất)

    [Column("point_multiplier")]
    public decimal PointMultiplier { get; set; } = 1.0m; // Hệ số nhân điểm khi mua vé

    [Column("discount_percentage")]
    public decimal DiscountPercentage { get; set; } = 0; // % giảm giá cho hội viên cấp này

    [Column("color", TypeName = "varchar(20)")]
    public string? Color { get; set; } // Màu đại diện cho UI

    [Column("icon_url", TypeName = "text")]
    public string? IconUrl { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("benefits", TypeName = "jsonb")]
    public string? Benefits { get; set; } // JSON array các quyền lợi

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
