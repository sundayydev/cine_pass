namespace BE_CinePass.Domain.Events;

public class OrderConfirmedEvent : IDomainEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public int TicketCount { get; set; }
}