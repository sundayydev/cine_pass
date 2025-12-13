namespace BE_CinePass.Shared.DTOs.Screen;

public class ScreenResponseDto
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public string? SeatMapLayout { get; set; }
}

