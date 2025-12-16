using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Screen;

public class GenerateSeatsDto
{
    [Required(ErrorMessage = "Số hàng ghế là bắt buộc")]
    [Range(1, 26, ErrorMessage = "Số hàng ghế phải từ 1 đến 26 (A-Z)")]
    public int Rows { get; set; }

    [Required(ErrorMessage = "Số ghế mỗi hàng là bắt buộc")]
    [Range(1, 50, ErrorMessage = "Số ghế mỗi hàng phải từ 1 đến 50")]
    public int SeatsPerRow { get; set; }

    /// <summary>
    /// Loại ghế mặc định (VD: "STANDARD", "VIP", "COUPLE")
    /// Sử dụng khi không có cấu hình RowSeatTypes
    /// </summary>
    [MaxLength(50)]
    public string? DefaultSeatTypeCode { get; set; }

    /// <summary>
    /// Cấu hình loại ghế theo hàng
    /// Key: Tên hàng (A, B, C, ...), Value: Mã loại ghế
    /// VD: { "A": "VIP", "B": "VIP", "C": "STANDARD" }
    /// </summary>
    public Dictionary<string, string>? RowSeatTypes { get; set; }
}
