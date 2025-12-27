using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("total_amount", TypeName = "numeric(12,2)")]
    public decimal TotalAmount { get; set; } = 0;

    [Column("user_voucher_id")]
    public Guid? UserVoucherId { get; set; } // Voucher được áp dụng cho order này

    [Column("discount_amount", TypeName = "numeric(12,2)")]
    public decimal DiscountAmount { get; set; } = 0; // Số tiền giảm giá từ voucher

    [Column("final_amount", TypeName = "numeric(12,2)")]
    public decimal FinalAmount { get; set; } = 0; // Tổng tiền sau khi giảm giá

    [Required]
    [Column("status")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [MaxLength(50)]
    [Column("payment_method")]
    public string? PaymentMethod { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expire_at")]
    public DateTime? ExpireAt { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [ForeignKey(nameof(UserVoucherId))]
    public virtual UserVoucher? UserVoucher { get; set; }

    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
