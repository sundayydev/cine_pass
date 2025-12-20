namespace BE_CinePass.Shared.DTOs.Momo;

/// <summary>
/// Request để tạo giao dịch thanh toán Momo
/// </summary>
public class CreateMomoPaymentRequest
{
    /// <summary>
    /// ID đơn hàng (Order ID)
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Số tiền thanh toán
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Thông tin đơn hàng
    /// </summary>
    public string OrderInfo { get; set; } = string.Empty;

    /// <summary>
    /// Ngôn ngữ (vi, en)
    /// </summary>
    public string Lang { get; set; } = "vi";

    /// <summary>
    /// Thông tin bổ sung (optional)
    /// </summary>
    public string? ExtraData { get; set; }
}
