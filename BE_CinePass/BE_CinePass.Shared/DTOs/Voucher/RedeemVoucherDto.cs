using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Voucher;

public class RedeemVoucherDto
{
    [Required(ErrorMessage = "Voucher ID là bắt buộc")]
    public Guid VoucherId { get; set; }
}
