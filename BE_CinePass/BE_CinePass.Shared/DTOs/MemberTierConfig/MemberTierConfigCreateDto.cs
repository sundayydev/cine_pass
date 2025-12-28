namespace BE_CinePass.Shared.DTOs.MemberTierConfig;

public class MemberTierConfigCreateDto
{
    public string Tier { get; set; } = string.Empty; // Bronze, Silver, Gold, Diamond
    public string Name { get; set; } = string.Empty;
    public int MinPoints { get; set; }
    public int? MaxPoints { get; set; }
    public decimal PointMultiplier { get; set; } = 1.0m;
    public decimal DiscountPercentage { get; set; } = 0m;
    public string? Color { get; set; }
    public string? IconUrl { get; set; }
    public string? Description { get; set; }
    public List<string>? Benefits { get; set; }
}
