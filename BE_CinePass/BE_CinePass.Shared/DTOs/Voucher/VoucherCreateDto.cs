using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Voucher;

public class VoucherCreateDto
{
    [Required(ErrorMessage = "Mã voucher là bắt buộc")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên voucher là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Loại voucher là bắt buộc")]
    public string Type { get; set; } = string.Empty; // Percentage, FixedAmount

    [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0")]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa phải lớn hơn hoặc bằng 0")]
    public decimal? MaxDiscountAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
    public decimal? MinOrderAmount { get; set; }

    [Required(ErrorMessage = "Số điểm cần đổi là bắt buộc")]
    [Range(0, int.MaxValue, ErrorMessage = "Số điểm cần đổi phải lớn hơn hoặc bằng 0")]
    public int PointsRequired { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int? Quantity { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public string? MinTier { get; set; } // Bronze, Silver, Gold, Diamond
}
