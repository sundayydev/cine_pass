namespace BE_CinePass.Shared.DTOs.MovieActor;

public class MovieActorResponseDto
{
    public Guid Id { get; set; }
    public Guid? MovieId { get; set; }
    public Guid? ActorId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Optional: Include related data
    public string? MovieTitle { get; set; }
    public string? ActorName { get; set; }
}
