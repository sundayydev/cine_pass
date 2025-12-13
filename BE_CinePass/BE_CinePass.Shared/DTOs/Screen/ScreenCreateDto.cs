using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Screen;

public class ScreenCreateDto
{
    [Required]
    public Guid CinemaId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int TotalSeats { get; set; } = 0;

    public string? SeatMapLayout { get; set; } // JSON string
}

