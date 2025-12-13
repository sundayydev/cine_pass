using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Showtime;

public class ShowtimeCreateDto
{
    [Required]
    public Guid MovieId { get; set; }

    [Required]
    public Guid ScreenId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;
}

