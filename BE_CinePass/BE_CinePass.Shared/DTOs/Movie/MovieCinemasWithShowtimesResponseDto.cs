namespace BE_CinePass.Shared.DTOs.Movie;

public class MovieCinemasWithShowtimesResponseDto
{
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? Category { get; set; }  // Thêm category
    public int? DurationMinutes { get; set; }  // Thêm duration
    public int? AgeLimit { get; set; }  // Thêm age limit
    public List<CinemaWithShowtimesForMovieDto> Cinemas { get; set; } = new();
}