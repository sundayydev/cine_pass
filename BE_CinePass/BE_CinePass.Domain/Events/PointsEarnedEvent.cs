namespace BE_CinePass.Domain.Events;

public class PointsEarnedEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public int PointsEarned { get; set; }
    public int BasePoints { get; set; }
    public int BonusPoints { get; set; }
    public int NewTotalPoints { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}