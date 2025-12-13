using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace BE_CinePass.Domain.Models;

[Table("payment_transactions")]
public class PaymentTransaction
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("order_id")]
    public Guid? OrderId { get; set; }

    [MaxLength(255)]
    [Column("provider_trans_id")]
    public string? ProviderTransId { get; set; }

    [Column("amount", TypeName = "numeric(12,2)")]
    public decimal? Amount { get; set; }

    [MaxLength(50)]
    [Column("status")]
    public string? Status { get; set; }

    [Column("response_json", TypeName = "jsonb")]
    public JsonDocument? ResponseJson { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }
}
