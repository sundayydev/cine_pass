using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Order;

public class ApplyVoucherDto
{
    [Required(ErrorMessage = "Order ID là bắt buộc")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "User Voucher ID là bắt buộc")]
    public Guid UserVoucherId { get; set; }
}
