using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("user_vouchers")]
public class UserVoucher
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("voucher_id")]
    public Guid VoucherId { get; set; }

    [Column("is_used")]
    public bool IsUsed { get; set; } = false;

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    [Column("order_id")]
    public Guid? OrderId { get; set; } // Order mà voucher được sử dụng

    [Column("redeemed_at")]
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow; // Thời điểm đổi voucher

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; } // Thời hạn sử dụng voucher

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("VoucherId")]
    public virtual Voucher Voucher { get; set; } = null!;

    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }
}
