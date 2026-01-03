namespace BE_CinePass.Domain.Events;

public class SecurityAlertEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}