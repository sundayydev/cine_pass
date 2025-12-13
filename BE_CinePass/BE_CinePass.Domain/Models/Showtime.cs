using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("showtimes")]
public class Showtime
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("movie_id")]
    public Guid MovieId { get; set; }

    [Required]
    [Column("screen_id")]
    public Guid ScreenId { get; set; }

    [Required]
    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Required]
    [Column("base_price", TypeName = "numeric(10,2)")]
    public decimal BasePrice { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey(nameof(MovieId))]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey(nameof(ScreenId))]
    public virtual Screen Screen { get; set; } = null!;

    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
}
