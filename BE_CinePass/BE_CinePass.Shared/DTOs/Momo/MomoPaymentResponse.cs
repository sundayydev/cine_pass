namespace BE_CinePass.Shared.DTOs.Momo;

/// <summary>
/// Response từ Momo API khi tạo giao dịch
/// </summary>
public class MomoPaymentResponse
{
    /// <summary>
    /// Partner Code
    /// </summary>
    public string PartnerCode { get; set; } = string.Empty;

    /// <summary>
    /// Order ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Request ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Số tiền
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Mã lỗi (0 = thành công)
    /// </summary>
    public int ResultCode { get; set; }

    /// <summary>
    /// Thông báo kết quả
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// URL thanh toán (để redirect hoặc tạo QR)
    /// </summary>
    public string PayUrl { get; set; } = string.Empty;

    /// <summary>
    /// Deeplink cho Momo app (ưu tiên hơn PayUrl)
    /// </summary>
    public string? Deeplink { get; set; }

    /// <summary>
    /// QR Code data (base64 hoặc URL)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Signature để verify
    /// </summary>
    public string Signature { get; set; } = string.Empty;
}
