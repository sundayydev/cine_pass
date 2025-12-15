using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("movie_reviews")]
public class MovieReview
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("movie_id")]
    public Guid? MovieId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("rating")]
    [Range(1, 5)]
    public int? Rating { get; set; }

    [Column("comment", TypeName = "text")]
    public string? Comment { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("MovieId")]
    public virtual Movie? Movie { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
