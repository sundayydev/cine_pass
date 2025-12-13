namespace BE_CinePass.Shared.DTOs.Showtime;

public class ShowtimeResponseDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid ScreenId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
}

