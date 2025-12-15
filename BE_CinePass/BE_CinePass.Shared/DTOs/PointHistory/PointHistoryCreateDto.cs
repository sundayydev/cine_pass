using BE_CinePass.Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.PointHistory;

public class PointHistoryCreateDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int Points { get; set; }

    [Required]
    public PointHistoryType Type { get; set; } = PointHistoryType.Purchase;
}
