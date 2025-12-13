using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace BE_CinePass.Domain.Models;

[Table("screens")]
public class Screen
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("cinema_id")]
    public Guid CinemaId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("total_seats")]
    public int TotalSeats { get; set; } = 0;

    [Column("seat_map_layout", TypeName = "jsonb")]
    public JsonDocument? SeatMapLayout { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CinemaId))]
    public virtual Cinema Cinema { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
