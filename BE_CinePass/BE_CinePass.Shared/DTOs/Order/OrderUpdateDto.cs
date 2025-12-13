using BE_CinePass.Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Order;

public class OrderUpdateDto
{
    public OrderStatus? Status { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }
}

