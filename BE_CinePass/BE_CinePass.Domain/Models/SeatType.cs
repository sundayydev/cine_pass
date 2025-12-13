using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("seat_types")]
public class SeatType
{
    [Key]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("name")]
    public string? Name { get; set; }

    [Column("surcharge_rate", TypeName = "numeric(5,2)")]
    public decimal SurchargeRate { get; set; } = 1.0m;

    // Navigation properties
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
