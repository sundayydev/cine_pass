using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Voucher;

public class VoucherUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Status { get; set; } // Active, Inactive, Expired
    
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int? Quantity { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
