using BE_CinePass.Shared.DTOs.Cinema;
using BE_CinePass.Shared.DTOs.Order;
using BE_CinePass.Shared.DTOs.Seat;
using BE_CinePass.Shared.DTOs.Screen;

namespace BE_CinePass.Shared.DTOs.ETicket;

public class ETicketDetailDto
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string? QrData { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public OrderTicketDetailDto OrderTicket { get; set; } = null!;

    // Thông tin rạp chiếu
    public CinemaInfoDto? Cinema { get; set; }

    // Thông tin phòng chiếu
    public ScreenInfoDto? Screen { get; set; }

    // Thông tin ghế
    public SeatInfoDto? Seat { get; set; }

    // Thông tin người mua vé
    public BuyerInfoDto? Buyer { get; set; }
}

/// <summary>
/// Thông tin rạp chiếu phim (cho check-in)
/// </summary>
public class CinemaInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
}

/// <summary>
/// Thông tin phòng chiếu (cho check-in)
/// </summary>
public class ScreenInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}

/// <summary>
/// Thông tin ghế ngồi (cho check-in)
/// </summary>
public class SeatInfoDto
{
    public Guid Id { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string SeatRow { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string? SeatTypeCode { get; set; }
}

/// <summary>
/// Thông tin người mua vé (cho check-in)
/// </summary>
public class BuyerInfoDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

