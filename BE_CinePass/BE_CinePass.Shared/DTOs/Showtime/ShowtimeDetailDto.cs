using BE_CinePass.Shared.DTOs.Movie;
using BE_CinePass.Shared.DTOs.Screen;

namespace BE_CinePass.Shared.DTOs.Showtime;

public class ShowtimeDetailDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public MovieResponseDto Movie { get; set; } = null!;
    public ScreenResponseDto Screen { get; set; } = null!;
}

