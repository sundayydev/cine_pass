using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Actor;

public class ActorCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Slug { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    [Url]
    public string? ImageUrl { get; set; }
}
