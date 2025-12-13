using BE_CinePass.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("movies")]
public class Movie
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("slug")]
    public string? Slug { get; set; }

    [Required]
    [Column("duration_minutes")]
    public int DurationMinutes { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [MaxLength(500)]
    [Column("poster_url")]
    public string? PosterUrl { get; set; }

    [MaxLength(500)]
    [Column("trailer_url")]
    public string? TrailerUrl { get; set; }

    [Column("release_date", TypeName = "date")]
    public DateTime? ReleaseDate { get; set; }
    [Column("category")]
    public MovieCategory Category { get; set; } = MovieCategory.Movie;
    [Required]
    [Column("status")]
    public MovieStatus Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
