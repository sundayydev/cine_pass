namespace BE_CinePass.Shared.DTOs.Momo;

/// <summary>
/// Response trả về cho client từ API tạo thanh toán
/// </summary>
public class CreateMomoPaymentResponse
{
    /// <summary>
    /// Có thành công không
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Thông báo
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// URL thanh toán (để redirect hoặc hiển thị QR)
    /// </summary>
    public string? PayUrl { get; set; }

    /// <summary>
    /// Deeplink Momo app (ưu tiên cho mobile)
    /// </summary>
    public string? Deeplink { get; set; }

    /// <summary>
    /// QR Code URL (nếu có)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Order ID
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// Request ID (để tracking)
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Mã lỗi từ Momo (0 = thành công)
    /// </summary>
    public int? ResultCode { get; set; }
}
