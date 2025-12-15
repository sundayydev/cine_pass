using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("actors")]
public class Actor
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("name", TypeName = "text")]
    public string Name { get; set; } = string.Empty;

    [Column("slug", TypeName = "text")]
    public string? Slug { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("image_url", TypeName = "text")]
    public string? ImageUrl { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
}
