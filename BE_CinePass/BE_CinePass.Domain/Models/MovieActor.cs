using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("movie_actors")]
public class MovieActor
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("movie_id")]
    public Guid? MovieId { get; set; }

    [Column("actor_id")]
    public Guid? ActorId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("MovieId")]
    public virtual Movie? Movie { get; set; }

    [ForeignKey("ActorId")]
    public virtual Actor? Actor { get; set; }
}
