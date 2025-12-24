namespace BE_CinePass.Shared.DTOs.ETicket;

public class TicketVerificationResultDto
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty; // "Valid", "Invalid", "AlreadyUsed", "Expired"
    public string Message { get; set; } = string.Empty;

    // Thời gian check-in
    public DateTime? CheckinAt { get; set; }

    // Thông tin vé chi tiết
    public ETicketDetailDto? TicketDetail { get; set; }

    // Thông tin tổng hợp nhanh cho hiển thị
    public CheckinSummaryDto? Summary { get; set; }
}

/// <summary>
/// Thông tin tổng hợp nhanh khi check-in
/// </summary>
public class CheckinSummaryDto
{
    // Thông tin vé
    public string TicketCode { get; set; } = string.Empty;
    public decimal TicketPrice { get; set; }

    // Thông tin phim
    public string MovieTitle { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int MovieDurationMinutes { get; set; }
    public string? MovieRating { get; set; } // Phân loại độ tuổi

    // Thông tin suất chiếu
    public DateTime ShowtimeStart { get; set; }
    public DateTime ShowtimeEnd { get; set; }
    public int MinutesUntilShowtime { get; set; } // Số phút đến suất chiếu (âm nếu đã qua)
    public bool IsShowtimeStarted { get; set; } // Suất chiếu đã bắt đầu chưa

    // Thông tin rạp & phòng chiếu
    public string CinemaName { get; set; } = string.Empty;
    public string? CinemaAddress { get; set; }
    public string ScreenName { get; set; } = string.Empty;

    // Thông tin ghế
    public string SeatCode { get; set; } = string.Empty;
    public string SeatRow { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string? SeatType { get; set; } // VIP, Standard, Couple, etc.

    // Thông tin khách hàng
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    // Thông tin đơn hàng liên quan
    public Guid OrderId { get; set; }
    public decimal OrderTotalAmount { get; set; }
    public int TotalTicketsInOrder { get; set; } // Tổng số vé trong đơn
    public int CheckedInTicketsInOrder { get; set; } // Số vé đã check-in trong đơn

    // Thông tin đồ ăn/uống đi kèm (nếu có)
    public List<OrderProductSummaryDto>? Products { get; set; }
}

/// <summary>
/// Thông tin sản phẩm đi kèm đơn hàng (đồ ăn/uống)
/// </summary>
public class OrderProductSummaryDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Category { get; set; } // Food, Drink, Combo, etc.
}

