using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("order_tickets")]
public class OrderTicket
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("showtime_id")]
    public Guid ShowtimeId { get; set; }

    [Required]
    [Column("seat_id")]
    public Guid SeatId { get; set; }

    [Required]
    [Column("price", TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    // Navigation properties
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(ShowtimeId))]
    public virtual Showtime Showtime { get; set; } = null!;

    [ForeignKey(nameof(SeatId))]
    public virtual Seat Seat { get; set; } = null!;

    public virtual ICollection<ETicket> ETickets { get; set; } = new List<ETicket>();
}
