using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Reward;

public class RewardUpdateDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Slug { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    [Url]
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public int? Points { get; set; }
}
