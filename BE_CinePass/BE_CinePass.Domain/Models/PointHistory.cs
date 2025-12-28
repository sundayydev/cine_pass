using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("point_history")]
public class PointHistory
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("points")]
    public int? Points { get; set; }

    [Column("type", TypeName = "text")]
    public PointHistoryType? Type { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("order_id")]
    public Guid? OrderId { get; set; } // Order liên quan (nếu có)

    [Column("voucher_id")]
    public Guid? VoucherId { get; set; } // Voucher được đổi (nếu type = RedeemVoucher)

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; } // Ngày hết hạn của điểm này (nếu có)

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    [ForeignKey("VoucherId")]
    public virtual Voucher? Voucher { get; set; }
}
