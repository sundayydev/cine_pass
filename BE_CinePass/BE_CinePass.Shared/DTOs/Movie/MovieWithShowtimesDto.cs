using BE_CinePass.Shared.DTOs.Showtime;

namespace BE_CinePass.Shared.DTOs.Movie;

public class MovieWithShowtimesDto
{
    public MovieResponseDto Movie { get; set; } = null!;
    public List<ShowtimeResponseDto> Showtimes { get; set; } = new List<ShowtimeResponseDto>();
}