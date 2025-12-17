namespace BE_CinePass.Shared.DTOs.Movie;

public class MovieCinemasWithShowtimesResponseDto
{
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public List<CinemaWithShowtimesForMovieDto> Cinemas { get; set; } = new();
}