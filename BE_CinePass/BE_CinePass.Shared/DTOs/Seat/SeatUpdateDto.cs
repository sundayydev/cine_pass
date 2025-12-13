using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Seat;

public class SeatUpdateDto
{
    [MaxLength(50)]
    public string? SeatTypeCode { get; set; }

    public bool? IsActive { get; set; }
}

