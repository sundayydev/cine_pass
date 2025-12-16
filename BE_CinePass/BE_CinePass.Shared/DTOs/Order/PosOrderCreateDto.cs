using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Order;

/// <summary>
/// DTO for creating POS (Point of Sale) orders with cash payment
/// </summary>
public class PosOrderCreateDto
{
    /// <summary>
    /// Số điện thoại khách hàng
    /// </summary>
    [Required]
    [Phone]
    [MaxLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Tên khách hàng
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Email khách hàng (optional)
    /// </summary>
    [EmailAddress]
    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Danh sách vé
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 vé")]
    public List<OrderTicketItemDto> Tickets { get; set; } = new();

    /// <summary>
    /// Danh sách sản phẩm (đồ ăn, nước uống)
    /// </summary>
    public List<OrderProductItemDto> Products { get; set; } = new();

    /// <summary>
    /// Số tiền khách đưa
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Số tiền khách đưa phải lớn hơn 0")]
    public decimal CashReceived { get; set; }

    /// <summary>
    /// Ghi chú của nhân viên
    /// </summary>
    [MaxLength(500)]
    public string? StaffNote { get; set; }
}
