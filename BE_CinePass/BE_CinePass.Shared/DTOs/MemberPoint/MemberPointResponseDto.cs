namespace BE_CinePass.Shared.DTOs.MemberPoint;

public class MemberPointResponseDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public int Points { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Diamond
    public string TierName { get; set; } = string.Empty;
    public int PointsToExpire { get; set; }
    public DateTime? NextExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Tier information
    public decimal PointMultiplier { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? TierColor { get; set; }
    
    // Progress to next tier
    public int? PointsToNextTier { get; set; }
    public string? NextTier { get; set; }
    public string? NextTierName { get; set; }

    // Optional: Include user info
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
}


