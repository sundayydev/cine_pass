namespace BE_CinePass.Shared.DTOs.OrderTicket;

public class OrderTicketResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ShowtimeId { get; set; }
    public Guid SeatId { get; set; }
    public decimal Price { get; set; }
}

