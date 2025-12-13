using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.SeatType;

public class SeatTypeCreateDto
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Name { get; set; }

    [Range(0.1, 10.0)]
    public decimal SurchargeRate { get; set; } = 1.0m;
}

