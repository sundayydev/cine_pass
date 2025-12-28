namespace BE_CinePass.Shared.DTOs.MemberTierConfig;

public class MemberTierConfigResponseDto
{
    public Guid Id { get; set; }
    public string Tier { get; set; } = string.Empty; // Bronze, Silver, Gold, Diamond
    public string Name { get; set; } = string.Empty;
    public int MinPoints { get; set; }
    public int? MaxPoints { get; set; }
    public decimal PointMultiplier { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? Color { get; set; }
    public string? IconUrl { get; set; }
    public string? Description { get; set; }
    public List<string>? Benefits { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
