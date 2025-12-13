using BE_CinePass.Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Movie;

public class MovieCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Slug { get; set; }

    [Required]
    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    [Url]
    public string? PosterUrl { get; set; }

    [MaxLength(500)]
    [Url]
    public string? TrailerUrl { get; set; }

    public DateTime? ReleaseDate { get; set; }

    [Required]
    public MovieStatus Status { get; set; }
}

