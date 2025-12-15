using BE_CinePass.Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Movie;

public class MovieUpdateDto
{
    [MaxLength(255)]
    public string? Title { get; set; }

    [MaxLength(255)]
    public string? Slug { get; set; }

    [Range(1, 600)]
    public int? DurationMinutes { get; set; }

    public string? Description { get; set; }

    [Range(0, 18)]
    public int? AgeLimit { get; set; }

    [MaxLength(500)]
    [Url]
    public string? PosterUrl { get; set; }

    [MaxLength(500)]
    [Url]
    public string? TrailerUrl { get; set; }

    public DateTime? ReleaseDate { get; set; }
    public MovieCategory? Category { get; set; }

    public MovieStatus? Status { get; set; }
}

