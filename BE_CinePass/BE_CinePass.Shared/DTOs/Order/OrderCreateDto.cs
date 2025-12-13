using BE_CinePass.Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Order;

public class OrderCreateDto
{
    public Guid? UserId { get; set; }

    [Required]
    public List<OrderTicketItemDto> Tickets { get; set; } = new();

    public List<OrderProductItemDto> Products { get; set; } = new();

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }
}

public class OrderTicketItemDto
{
    [Required]
    public Guid ShowtimeId { get; set; }

    [Required]
    public Guid SeatId { get; set; }
}

public class OrderProductItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

