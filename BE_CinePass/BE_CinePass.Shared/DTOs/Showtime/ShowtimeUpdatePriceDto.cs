using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Showtime;

public class ShowtimeUpdatePriceDto
{
    [Required(ErrorMessage = "Giá vé cơ bản là bắt buộc")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá vé cơ bản phải lớn hơn 0")]
    public decimal BasePrice { get; set; }
}
