namespace BE_CinePass.Domain.Events;

public class OrderFailedEvent : IDomainEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}