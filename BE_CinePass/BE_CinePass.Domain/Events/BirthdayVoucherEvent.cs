namespace BE_CinePass.Domain.Events;

public class BirthdayVoucherEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string VoucherCode { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}