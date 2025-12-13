using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("e_tickets")]
public class ETicket
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("order_ticket_id")]
    public Guid OrderTicketId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("ticket_code")]
    public string TicketCode { get; set; } = string.Empty;

    [Column("qr_data", TypeName = "text")]
    public string? QrData { get; set; }

    [Column("is_used")]
    public bool IsUsed { get; set; } = false;

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(OrderTicketId))]
    public virtual OrderTicket OrderTicket { get; set; } = null!;
}
