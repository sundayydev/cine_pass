using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Showtime;

public class ShowtimeUpdateDto
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [Range(0.01, 999999.99)]
    public decimal? BasePrice { get; set; }

    public bool? IsActive { get; set; }
}

