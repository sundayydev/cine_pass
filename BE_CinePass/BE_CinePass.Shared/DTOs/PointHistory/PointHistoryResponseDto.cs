namespace BE_CinePass.Shared.DTOs.PointHistory;

public class PointHistoryResponseDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public int? Points { get; set; }
    public string? Type { get; set; } // 'Purchase', 'Refund', 'Reward', 'RedeemVoucher', 'Expired', 'Adjustment'
    public string? Description { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? VoucherId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Optional: Include user info
    public string? UserEmail { get; set; }
}
