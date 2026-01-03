namespace BE_CinePass.Domain.Events;

public class ShowtimeCreatedEvent : IDomainEvent
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public Guid CinemaId { get; set; }
    public Guid ScreenId { get; set; }
    public string ScreenName { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}