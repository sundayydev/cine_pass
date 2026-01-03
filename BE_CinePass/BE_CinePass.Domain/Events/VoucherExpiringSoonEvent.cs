namespace BE_CinePass.Domain.Events;

public class VoucherExpiringSoonEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    public string VoucherCode { get; set; } = string.Empty;
    public string VoucherName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int DaysUntilExpiry { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}