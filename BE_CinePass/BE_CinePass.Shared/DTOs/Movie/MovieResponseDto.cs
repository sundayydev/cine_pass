using BE_CinePass.Shared.Common;

namespace BE_CinePass.Shared.DTOs.Movie;

public class MovieResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public int AgeLimit { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double? AverageRating { get; set; }
    public int? TotalReviews { get; set; }
}

