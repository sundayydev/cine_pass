using BE_CinePass.Shared.Common;

namespace BE_CinePass.Shared.DTOs.Seat;

public class SeatWithStatusDto
{
    public Guid Id { get; set; }
    public string SeatRow { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string? SeatTypeCode { get; set; }
    public SeatStatus Status { get; set; }
    public decimal Price { get; set; }
    public string? HeldByUserId { get; set; }
}
