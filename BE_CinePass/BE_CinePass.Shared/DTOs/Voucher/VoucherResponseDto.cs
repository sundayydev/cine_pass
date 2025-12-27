namespace BE_CinePass.Shared.DTOs.Voucher;

public class VoucherResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Type { get; set; } = string.Empty; // Percentage, FixedAmount
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int PointsRequired { get; set; }
    public int? Quantity { get; set; }
    public int QuantityRedeemed { get; set; }
    public int? RemainingQuantity { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Expired
    public string? MinTier { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Additional fields for user view
    public bool CanRedeem { get; set; }
    public string? ReasonCannotRedeem { get; set; }
}
