namespace BE_CinePass.Shared.Settings;

/// <summary>
/// Cấu hình Momo Payment Gateway
/// </summary>
public class MomoSettings
{
    /// <summary>
    /// Mã đối tác do Momo cung cấp
    /// </summary>
    public string PartnerCode { get; set; } = string.Empty;

    /// <summary>
    /// Access key để xác thực
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret key để ký HMAC SHA256
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// URL redirect sau khi thanh toán (người dùng sẽ thấy)
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL nhận thông báo kết quả thanh toán từ Momo (IPN - Instant Payment Notification)
    /// </summary>
    public string IpnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Loại yêu cầu thanh toán
    /// - captureWallet: Thanh toán từ ví Momo
    /// - payWithATM: Thanh toán qua ATM
    /// - payWithCC: Thanh toán qua thẻ tín dụng
    /// </summary>
    public string RequestType { get; set; } = "captureWallet";

    /// <summary>
    /// Endpoint tạo giao dịch thanh toán
    /// </summary>
    public string ApiEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint truy vấn trạng thái giao dịch
    /// </summary>
    public string QueryEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint cho thanh toán POS (scan mã QR của khách)
    /// </summary>
    public string PosEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Môi trường: Development, Staging, Production
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Kiểm tra xem tất cả các cấu hình bắt buộc đã được thiết lập chưa
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(PartnerCode) &&
               !string.IsNullOrWhiteSpace(AccessKey) &&
               !string.IsNullOrWhiteSpace(SecretKey) &&
               !string.IsNullOrWhiteSpace(ApiEndpoint) &&
               !string.IsNullOrWhiteSpace(QueryEndpoint);
    }
}
