using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Seat;

public class SeatCreateDto
{
    [Required]
    public Guid ScreenId { get; set; }

    [Required]
    [MaxLength(10)]
    public string SeatRow { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int SeatNumber { get; set; }

    [Required]
    [MaxLength(20)]
    public string SeatCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SeatTypeCode { get; set; }

    public bool IsActive { get; set; } = true;
}

