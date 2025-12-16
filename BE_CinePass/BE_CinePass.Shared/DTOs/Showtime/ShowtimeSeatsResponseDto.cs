using BE_CinePass.Shared.DTOs.Seat;

namespace BE_CinePass.Shared.DTOs.Showtime;

public class ShowtimeSeatsResponseDto
{
    public Guid ShowtimeId { get; set; }
    public Guid ScreenId { get; set; }
    public string ScreenName { get; set; } = string.Empty;
    public DateTime ShowDateTime { get; set; }
    public List<SeatWithStatusDto> Seats { get; set; } = new();
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int SoldSeats { get; set; }
    public int HoldingSeats { get; set; }
}
