using BE_CinePass.Shared.DTOs.Showtime;

namespace BE_CinePass.Shared.DTOs.Movie;

public class CinemaWithShowtimesForMovieDto
{
    public Guid CinemaId { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Phone { get; set; }
    public string? BannerUrl { get; set; }
    public int TotalScreens { get; set; }
    public List<ShowtimeResponseDto> Showtimes { get; set; } = new();
}