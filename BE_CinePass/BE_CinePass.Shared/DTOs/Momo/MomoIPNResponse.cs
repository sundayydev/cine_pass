namespace BE_CinePass.Shared.DTOs.Momo;

/// <summary>
/// Response trả về cho Momo khi xử lý IPN
/// </summary>
public class MomoIPNResponse
{
    public string PartnerCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string TransId { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ResponseTime { get; set; } = string.Empty;
    public string ExtraData { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
