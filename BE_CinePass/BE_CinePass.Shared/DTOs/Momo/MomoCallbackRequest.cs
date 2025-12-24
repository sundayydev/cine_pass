namespace BE_CinePass.Shared.DTOs.Momo;

/// <summary>
/// Request từ Momo gửi về khi thanh toán hoàn tất (IPN/Callback)
/// </summary>
public class MomoCallbackRequest
{
    public string PartnerCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string? TransId { get; set; }  // Nullable vì có thể empty khi lỗi
    public int ResultCode { get; set; }
    public string? Message { get; set; }  // Nullable
    public string? PayType { get; set; }  // Nullable
    public long ResponseTime { get; set; }
    public string? ExtraData { get; set; }  // Nullable vì có thể empty
    public string Signature { get; set; } = string.Empty;
}
