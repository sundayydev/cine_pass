using BE_CinePass.Shared.DTOs.User;
using BE_CinePass.Shared.DTOs.Showtime;
using BE_CinePass.Shared.DTOs.Seat;
using BE_CinePass.Shared.DTOs.Product;

namespace BE_CinePass.Shared.DTOs.Order;

public class OrderDetailDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public UserResponseDto? User { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public List<OrderTicketDetailDto> Tickets { get; set; } = new();
    public List<OrderProductDetailDto> Products { get; set; } = new();
}

public class OrderTicketDetailDto
{
    public Guid Id { get; set; }
    public Guid ShowtimeId { get; set; }
    public Guid SeatId { get; set; }
    public decimal Price { get; set; }
    public ShowtimeDetailDto? Showtime { get; set; }
    public SeatResponseDto? Seat { get; set; }
}

public class OrderProductDetailDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductResponseDto? Product { get; set; }
}

