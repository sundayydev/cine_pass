namespace BE_CinePass.Domain.Events;

public class ShowtimeCancelledEvent : IDomainEvent
{
    public Guid ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public List<Guid> AffectedUserIds { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public DateTime OriginalStartTime { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}