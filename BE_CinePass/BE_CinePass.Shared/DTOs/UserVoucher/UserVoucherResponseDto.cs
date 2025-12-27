namespace BE_CinePass.Shared.DTOs.UserVoucher;

public class UserVoucherResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VoucherId { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime RedeemedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Voucher information
    public string VoucherCode { get; set; } = string.Empty;
    public string VoucherName { get; set; } = string.Empty;
    public string? VoucherDescription { get; set; }
    public string? VoucherImageUrl { get; set; }
    public string VoucherType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateTime? VoucherValidTo { get; set; }
    
    // Status
    public bool IsExpired { get; set; }
    public bool CanUse { get; set; }
    public string? ReasonCannotUse { get; set; }
}
