using BE_CinePass.Shared.Common;

namespace BE_CinePass.Shared.DTOs.Order;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? UserVoucherId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
}
