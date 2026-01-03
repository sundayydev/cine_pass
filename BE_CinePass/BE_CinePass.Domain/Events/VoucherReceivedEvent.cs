namespace BE_CinePass.Domain.Events;

public class VoucherReceivedEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    public Guid VoucherId { get; set; }
    public string VoucherCode { get; set; } = string.Empty;
    public string VoucherName { get; set; } = string.Empty;
    public decimal DiscountValue  { get; set; }
    public string VoucherType { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}