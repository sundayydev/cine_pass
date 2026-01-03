namespace BE_CinePass.Domain.Events;

public class MovieReleasedEvent : IDomainEvent
{
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}