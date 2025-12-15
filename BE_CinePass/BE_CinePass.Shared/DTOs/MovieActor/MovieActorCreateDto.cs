using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.MovieActor;

public class MovieActorCreateDto
{
    [Required]
    public Guid MovieId { get; set; }

    [Required]
    public Guid ActorId { get; set; }
}
