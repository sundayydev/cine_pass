namespace BE_CinePass.Shared.DTOs.Seat;

public class SeatResponseDto
{
    public Guid Id { get; set; }
    public Guid ScreenId { get; set; }
    public string SeatRow { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string? SeatTypeCode { get; set; }
    public string QrOrderingCode { get; set; }
    public bool IsActive { get; set; }
}

