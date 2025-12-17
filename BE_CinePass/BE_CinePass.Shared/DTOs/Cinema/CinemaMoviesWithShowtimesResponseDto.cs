using BE_CinePass.Shared.DTOs.Movie;

namespace BE_CinePass.Shared.DTOs.Cinema;

public class CinemaMoviesWithShowtimesResponseDto
{
    public Guid CinemaId { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public string? Slug { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<MovieWithShowtimesDto> Movies { get; set; } = new List<MovieWithShowtimesDto>();
}