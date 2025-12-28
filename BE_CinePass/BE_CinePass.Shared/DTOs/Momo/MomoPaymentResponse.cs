namespace BE_CinePass.Shared.DTOs.Momo;

/// <summary>
/// Response từ Momo API khi tạo giao dịch
/// </summary>
public class MomoPaymentResponse
{

    public string PartnerCode { get; set; } = string.Empty;

   
    public string OrderId { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;
    public long Amount { get; set; }

    public int ResultCode { get; set; }


    public string Message { get; set; } = string.Empty;


    public string PayUrl { get; set; } = string.Empty;


    public string? Deeplink { get; set; }

 
    public string? QrCodeUrl { get; set; }

 
    public string Signature { get; set; } = string.Empty;
}
