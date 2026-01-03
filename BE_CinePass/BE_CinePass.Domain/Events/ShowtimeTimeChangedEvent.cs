namespace BE_CinePass.Domain.Events;

public class ShowtimeTimeChangedEvent : IDomainEvent
{
    public Guid ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public DateTime OldStartTime { get; set; }
    public string ScreenName { get; set; }
    public DateTime NewStartTime { get; set; }
    public List<Guid> AffectedUserIds { get; set; } = new();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}