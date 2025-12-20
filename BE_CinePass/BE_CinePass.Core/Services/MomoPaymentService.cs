using BE_CinePass.Shared.Settings;
using BE_CinePass.Shared.DTOs.Momo;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BE_CinePass.Core.Services;

/// <summary>
/// Service xử lý thanh toán Momo
/// </summary>
public class MomoPaymentService
{
    private readonly MomoSettings _momoSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MomoPaymentService> _logger;

    public MomoPaymentService(
        IOptions<MomoSettings> momoSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<MomoPaymentService> logger)
    {
        _momoSettings = momoSettings.Value;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;

        // Validate cấu hình
        if (!_momoSettings.IsValid())
        {
            throw new InvalidOperationException(
                "Momo settings are not configured properly. " +
                "Please check your .env file and ensure all required MOMO_* variables are set.");
        }
    }

    /// <summary>
    /// Tạo signature HMAC SHA256 cho request Momo
    /// </summary>
    private string CreateSignature(string rawData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_momoSettings.SecretKey);
        var messageBytes = Encoding.UTF8.GetBytes(rawData);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes)
            .Replace("-", "")
            .ToLower();
    }

    /// <summary>
    /// Tạo request ID duy nhất
    /// </summary>
    private string GenerateRequestId()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Tạo giao dịch thanh toán Momo
    /// </summary>
    public async Task<MomoPaymentResponse> CreatePaymentAsync(
        Guid orderId,
        decimal amount,
        string orderInfo,
        string? extraData = null)
    {
        try
        {
            var momoOrderId = $"ORDER_{orderId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var requestId = GenerateRequestId();
            var amountLong = (long)amount;

            // Tạo raw signature theo thứ tự của Momo
            var rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                              $"&amount={amountLong}" +
                              $"&extraData={extraData ?? ""}" +
                              $"&ipnUrl={_momoSettings.IpnUrl}" +
                              $"&orderId={momoOrderId}" +
                              $"&orderInfo={orderInfo}" +
                              $"&partnerCode={_momoSettings.PartnerCode}" +
                              $"&redirectUrl={_momoSettings.ReturnUrl}" +
                              $"&requestId={requestId}" +
                              $"&requestType={_momoSettings.RequestType}";

            var signature = CreateSignature(rawSignature);

            // Tạo request body
            var requestBody = new
            {
                partnerCode = _momoSettings.PartnerCode,
                partnerName = "CinePass",
                storeId = "CinePass",
                requestId = requestId,
                amount = amountLong,
                orderId = momoOrderId,
                orderInfo = orderInfo,
                redirectUrl = _momoSettings.ReturnUrl,
                ipnUrl = _momoSettings.IpnUrl,
                lang = "vi",
                extraData = extraData ?? "",
                requestType = _momoSettings.RequestType,
                signature = signature
            };

            _logger.LogInformation("Creating Momo payment for Order #{OrderId}, Amount: {Amount}",
                orderId, amount);

            // Gửi request đến Momo
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(_momoSettings.ApiEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Momo API Response: {Response}", responseContent);

            var momoResponse = JsonSerializer.Deserialize<MomoPaymentResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (momoResponse == null)
            {
                throw new Exception("Failed to parse Momo response");
            }

            return momoResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Momo payment for Order #{OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Xác thực signature từ Momo callback/IPN
    /// </summary>
    public bool VerifySignature(MomoCallbackRequest callback)
    {
        try
        {
            var rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                              $"&amount={callback.Amount}" +
                              $"&extraData={callback.ExtraData}" +
                              $"&message={callback.Message}" +
                              $"&orderId={callback.OrderId}" +
                              $"&orderInfo={callback.OrderInfo}" +
                              $"&orderType={callback.OrderType}" +
                              $"&partnerCode={callback.PartnerCode}" +
                              $"&payType={callback.PayType}" +
                              $"&requestId={callback.RequestId}" +
                              $"&responseTime={callback.ResponseTime}" +
                              $"&resultCode={callback.ResultCode}" +
                              $"&transId={callback.TransId}";

            var expectedSignature = CreateSignature(rawSignature);
            var isValid = expectedSignature.Equals(callback.Signature, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                _logger.LogWarning("Invalid Momo signature. Expected: {Expected}, Got: {Actual}",
                    expectedSignature, callback.Signature);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Momo signature");
            return false;
        }
    }

    /// <summary>
    /// Truy vấn trạng thái giao dịch từ Momo
    /// </summary>
    public async Task<MomoPaymentResponse> QueryTransactionAsync(string orderId, string requestId)
    {
        try
        {
            var rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                              $"&orderId={orderId}" +
                              $"&partnerCode={_momoSettings.PartnerCode}" +
                              $"&requestId={requestId}";

            var signature = CreateSignature(rawSignature);

            var requestBody = new
            {
                partnerCode = _momoSettings.PartnerCode,
                requestId = requestId,
                orderId = orderId,
                lang = "vi",
                signature = signature
            };

            _logger.LogInformation("Querying Momo transaction for Order: {OrderId}", orderId);

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(_momoSettings.QueryEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Momo Query Response: {Response}", responseContent);

            var momoResponse = JsonSerializer.Deserialize<MomoPaymentResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (momoResponse == null)
            {
                throw new Exception("Failed to parse Momo query response");
            }

            return momoResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Momo transaction for Order: {OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra môi trường hiện tại
    /// </summary>
    public bool IsProduction()
    {
        return _momoSettings.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Lấy thông tin cấu hình Momo (cho debugging)
    /// </summary>
    public object GetConfigInfo()
    {
        return new
        {
            IsValid = _momoSettings.IsValid(),
            Environment = _momoSettings.Environment,
            PartnerCode = _momoSettings.PartnerCode,
            HasAccessKey = !string.IsNullOrEmpty(_momoSettings.AccessKey),
            HasSecretKey = !string.IsNullOrEmpty(_momoSettings.SecretKey),
            RequestType = _momoSettings.RequestType,
            ApiEndpoint = _momoSettings.ApiEndpoint,
            QueryEndpoint = _momoSettings.QueryEndpoint,
            PosEndpoint = _momoSettings.PosEndpoint,
            ReturnUrl = _momoSettings.ReturnUrl,
            IpnUrl = _momoSettings.IpnUrl
        };
    }
}
