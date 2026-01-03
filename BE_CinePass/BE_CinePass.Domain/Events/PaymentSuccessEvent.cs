namespace BE_CinePass.Domain.Events;

public class PaymentSuccessEvent : IDomainEvent
{
    public Guid OrderId { get; set; }
    public Guid? UserId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}