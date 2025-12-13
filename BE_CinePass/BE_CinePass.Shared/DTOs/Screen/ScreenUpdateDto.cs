using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Screen;

public class ScreenUpdateDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [Range(0, int.MaxValue)]
    public int? TotalSeats { get; set; }

    public string? SeatMapLayout { get; set; } // JSON string
}

