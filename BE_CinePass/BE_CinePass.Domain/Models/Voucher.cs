using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("vouchers")]
public class Voucher
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("code", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Column("name", TypeName = "text")]
    public string Name { get; set; } = string.Empty;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("image_url", TypeName = "text")]
    public string? ImageUrl { get; set; }

    [Required]
    [Column("type", TypeName = "text")]
    public VoucherType Type { get; set; } // Percentage or FixedAmount

    [Required]
    [Column("discount_value")]
    public decimal DiscountValue { get; set; } // Giá trị giảm (% hoặc số tiền)

    [Column("max_discount_amount")]
    public decimal? MaxDiscountAmount { get; set; } // Số tiền giảm tối đa (áp dụng cho loại %)

    [Column("min_order_amount")]
    public decimal? MinOrderAmount { get; set; } // Giá trị đơn hàng tối thiểu để áp dụng

    [Required]
    [Column("points_required")]
    public int PointsRequired { get; set; } // Số điểm cần để đổi voucher này

    [Column("quantity")]
    public int? Quantity { get; set; } // Số lượng voucher (null = unlimited)

    [Column("quantity_redeemed")]
    public int QuantityRedeemed { get; set; } = 0; // Số lượng đã được đổi

    [Column("valid_from")]
    public DateTime? ValidFrom { get; set; }

    [Column("valid_to")]
    public DateTime? ValidTo { get; set; }

    [Required]
    [Column("status", TypeName = "text")]
    public VoucherStatus Status { get; set; } = VoucherStatus.Active;

    [Column("min_tier", TypeName = "text")]
    public MemberTier? MinTier { get; set; } // Cấp bậc tối thiểu để đổi voucher

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
}
