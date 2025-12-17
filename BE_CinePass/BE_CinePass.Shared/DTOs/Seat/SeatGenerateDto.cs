using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Seat;

/// <summary>
/// DTO cho việc tự động tạo ghế
/// </summary>
public class SeatGenerateDto
{
    /// <summary>
    /// ID của màn hình
    /// </summary>
    [Required(ErrorMessage = "ScreenId là bắt buộc")]
    public Guid ScreenId { get; set; }

    /// <summary>
    /// Số hàng ghế
    /// </summary>
    [Required(ErrorMessage = "Số hàng ghế là bắt buộc")]
    [Range(1, 26, ErrorMessage = "Số hàng ghế phải từ 1 đến 26")]
    public int Rows { get; set; }

    /// <summary>
    /// Số ghế mỗi hàng
    /// </summary>
    [Required(ErrorMessage = "Số ghế mỗi hàng là bắt buộc")]
    [Range(1, 50, ErrorMessage = "Số ghế mỗi hàng phải từ 1 đến 50")]
    public int SeatsPerRow { get; set; }

    /// <summary>
    /// Mã loại ghế mặc định (tùy chọn)
    /// </summary>
    public string? DefaultSeatTypeCode { get; set; }
}
