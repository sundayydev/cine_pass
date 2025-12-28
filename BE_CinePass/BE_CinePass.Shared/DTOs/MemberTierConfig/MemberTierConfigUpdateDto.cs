namespace BE_CinePass.Shared.DTOs.MemberTierConfig;

public class MemberTierConfigUpdateDto
{
    public string? Name { get; set; }
    public int? MinPoints { get; set; }
    public int? MaxPoints { get; set; }
    public decimal? PointMultiplier { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string? Color { get; set; }
    public string? IconUrl { get; set; }
    public string? Description { get; set; }
    public List<string>? Benefits { get; set; }
}
