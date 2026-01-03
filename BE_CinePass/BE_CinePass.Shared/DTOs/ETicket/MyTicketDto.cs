namespace BE_CinePass.Shared.DTOs.ETicket;

/// <summary>
/// DTO tối ưu để lấy danh sách vé của user, chỉ chứa các trường cần thiết cho hiển thị
/// </summary>
public class MyTicketDto
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string? QrData { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Movie info
    public string MovieTitle { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int MovieDurationMinutes { get; set; }
    public string? MovieCategory { get; set; }
    public int? MovieAgeLimit { get; set; }
    
    // Showtime info
    public DateTime ShowtimeStart { get; set; }
    public DateTime? ShowtimeEnd { get; set; }
    
    // Cinema & Screen info  
    public string CinemaName { get; set; } = string.Empty;
    public string? CinemaAddress { get; set; }
    public string ScreenName { get; set; } = string.Empty;
    
    // Seat info
    public string SeatCode { get; set; } = string.Empty;
    public string? SeatType { get; set; }
}
