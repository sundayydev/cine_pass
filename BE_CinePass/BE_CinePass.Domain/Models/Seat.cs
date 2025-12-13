using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("seats")]
public class Seat
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("screen_id")]
    public Guid ScreenId { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("seat_row")]
    public string SeatRow { get; set; } = string.Empty;

    [Required]
    [Column("seat_number")]
    public int SeatNumber { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("seat_code")]
    public string SeatCode { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("seat_type_code")]
    public string? SeatTypeCode { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey(nameof(ScreenId))]
    public virtual Screen Screen { get; set; } = null!;

    [ForeignKey(nameof(SeatTypeCode))]
    public virtual SeatType? SeatType { get; set; }

    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
}
