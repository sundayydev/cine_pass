using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Momo;
using BE_CinePass.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

/// <summary>
/// Controller xử lý thanh toán Momo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MomoPaymentController : ControllerBase
{
    private readonly MomoPaymentService _momoService;
    private readonly OrderService _orderService;
    private readonly PaymentTransactionService _paymentTransactionService;
    private readonly ILogger<MomoPaymentController> _logger;

    public MomoPaymentController(
        MomoPaymentService momoService,
        OrderService orderService,
        PaymentTransactionService paymentTransactionService,
        ILogger<MomoPaymentController> logger)
    {
        _momoService = momoService;
        _orderService = orderService;
        _paymentTransactionService = paymentTransactionService;
        _logger = logger;
    }

    /// <summary>
    /// Tạo giao dịch thanh toán Momo
    /// </summary>
    /// <param name="request">Thông tin thanh toán</param>
    /// <returns>URL thanh toán hoặc QR code</returns>
    [HttpPost("create")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CreateMomoPaymentResponse>> CreatePayment(
        [FromBody] CreateMomoPaymentRequest request)
    {
        try
        {
            // Kiểm tra order có tồn tại không
            var order = await _orderService.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return NotFound(new CreateMomoPaymentResponse
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng"
                });
            }

            // Kiểm tra order có thuộc về user hiện tại không
            var currentUserIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                return Unauthorized();
            }

            if (order.UserId != currentUserId)
            {
                return Forbid();
            }

            // Kiểm tra trạng thái order
            if (order.Status != "Pending")
            {
                return BadRequest(new CreateMomoPaymentResponse
                {
                    Success = false,
                    Message = $"Đơn hàng đang ở trạng thái {order.Status}, không thể thanh toán"
                });
            }

            // Tạo payment transaction
            var transaction = await _paymentTransactionService.CreateAsync(new Shared.DTOs.PaymentTransaction.PaymentTransactionCreateDto
            {
                OrderId = request.OrderId,
                ProviderTransId = null,
                Amount = request.Amount,
                Status = "Pending"
            });

            // Tạo thanh toán Momo
            var momoResponse = await _momoService.CreatePaymentAsync(
                request.OrderId,
                request.Amount,
                request.OrderInfo,
                request.ExtraData
            );

            // Kiểm tra kết quả từ Momo
            if (momoResponse.ResultCode != 0)
            {
                _logger.LogWarning("Momo payment failed for Order #{OrderId}: {Message}",
                    request.OrderId, momoResponse.Message);

                // Cập nhật transaction thành failed
                await _paymentTransactionService.UpdateStatusAsync(transaction.Id, "Failed");

                return BadRequest(new CreateMomoPaymentResponse
                {
                    Success = false,
                    Message = momoResponse.Message,
                    ResultCode = momoResponse.ResultCode
                });
            }

            // Cập nhật transaction với thông tin từ Momo
            await _paymentTransactionService.UpdateTransactionIdAsync(
                transaction.Id,
                momoResponse.RequestId
            );

            _logger.LogInformation("Momo payment created successfully for Order #{OrderId}", request.OrderId);

            return Ok(new CreateMomoPaymentResponse
            {
                Success = true,
                Message = "Tạo thanh toán thành công",
                PayUrl = momoResponse.PayUrl,
                Deeplink = momoResponse.Deeplink,
                QrCodeUrl = momoResponse.QrCodeUrl,
                OrderId = momoResponse.OrderId,
                RequestId = momoResponse.RequestId,
                ResultCode = momoResponse.ResultCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Momo payment for Order #{OrderId}", request.OrderId);
            return StatusCode(500, new CreateMomoPaymentResponse
            {
                Success = false,
                Message = "Có lỗi xảy ra khi tạo thanh toán. Vui lòng thử lại."
            });
        }
    }

    /// <summary>
    /// Callback từ Momo sau khi khách hàng thanh toán (Return URL)
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] MomoCallbackRequest callback)
    {
        try
        {
            _logger.LogInformation("Momo callback received: {Callback}",
                System.Text.Json.JsonSerializer.Serialize(callback));

            // Verify signature
            if (!_momoService.VerifySignature(callback))
            {
                _logger.LogWarning("Invalid Momo callback signature");
                return BadRequest("Invalid signature");
            }

            // Xử lý kết quả thanh toán
            await ProcessPaymentResult(callback);

            // Redirect về trang kết quả thanh toán trên frontend
            var frontendUrl = callback.ResultCode == 0
                ? $"{GetFrontendUrl()}/payment/success?orderId={callback.OrderId}"
                : $"{GetFrontendUrl()}/payment/failed?orderId={callback.OrderId}&message={callback.Message}";

            return Redirect(frontendUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Momo callback");
            return Redirect($"{GetFrontendUrl()}/payment/error");
        }
    }

    /// <summary>
    /// IPN (Instant Payment Notification) từ Momo
    /// </summary>
    [HttpPost("ipn")]
    [AllowAnonymous]
    public async Task<ActionResult<MomoIPNResponse>> IPN([FromBody] MomoCallbackRequest callback)
    {
        try
        {
            _logger.LogInformation("Momo IPN received: {Callback}",
                System.Text.Json.JsonSerializer.Serialize(callback));

            // Verify signature
            if (!_momoService.VerifySignature(callback))
            {
                _logger.LogWarning("Invalid Momo IPN signature");
                return Ok(new MomoIPNResponse
                {
                    PartnerCode = callback.PartnerCode,
                    OrderId = callback.OrderId,
                    RequestId = callback.RequestId,
                    Amount = callback.Amount,
                    OrderInfo = callback.OrderInfo,
                    OrderType = callback.OrderType,
                    TransId = callback.TransId,
                    ResultCode = 97, // Invalid signature
                    Message = "Invalid signature",
                    ResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                    ExtraData = callback.ExtraData,
                    Signature = ""
                });
            }

            // Xử lý kết quả thanh toán
            await ProcessPaymentResult(callback);

            // Trả về response cho Momo
            return Ok(new MomoIPNResponse
            {
                PartnerCode = callback.PartnerCode,
                OrderId = callback.OrderId,
                RequestId = callback.RequestId,
                Amount = callback.Amount,
                OrderInfo = callback.OrderInfo,
                OrderType = callback.OrderType,
                TransId = callback.TransId,
                ResultCode = 0, // Đã xử lý thành công
                Message = "Success",
                ResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                ExtraData = callback.ExtraData,
                Signature = ""
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Momo IPN");
            return Ok(new MomoIPNResponse
            {
                PartnerCode = callback.PartnerCode,
                OrderId = callback.OrderId,
                RequestId = callback.RequestId,
                Amount = callback.Amount,
                OrderInfo = callback.OrderInfo,
                OrderType = callback.OrderType,
                TransId = callback.TransId,
                ResultCode = 99, // Unknown error
                Message = "Internal error",
                ResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                ExtraData = callback.ExtraData,
                Signature = ""
            });
        }
    }

    /// <summary>
    /// Truy vấn trạng thái giao dịch
    /// </summary>
    [HttpGet("query/{orderId}")]
    [Authorize]
    public async Task<ActionResult<MomoPaymentResponse>> QueryTransaction(string orderId, [FromQuery] string requestId)
    {
        try
        {
            var result = await _momoService.QueryTransactionAsync(orderId, requestId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Momo transaction");
            return StatusCode(500, new { message = "Có lỗi xảy ra khi truy vấn giao dịch" });
        }
    }

    /// <summary>
    /// Lấy thông tin cấu hình Momo (for debugging)
    /// </summary>
    [HttpGet("config")]
    [Authorize(Roles = "Admin")]
    public ActionResult<object> GetConfig()
    {
        return Ok(_momoService.GetConfigInfo());
    }

    /// <summary>
    /// Xử lý kết quả thanh toán từ callback/IPN
    /// </summary>
    private async Task ProcessPaymentResult(MomoCallbackRequest callback)
    {
        // Parse order ID từ Momo order ID (format: ORDER_{orderId}_{timestamp})
        var parts = callback.OrderId.Split('_');
        if (parts.Length < 2 || !Guid.TryParse(parts[1], out var orderId))
        {
            _logger.LogWarning("Invalid order ID format: {OrderId}", callback.OrderId);
            return;
        }

        // Lấy transaction
        var transaction = await _paymentTransactionService.GetByTransactionIdAsync(callback.RequestId);
        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for RequestId: {RequestId}", callback.RequestId);
            return;
        }

        // Kiểm tra transaction đã được xử lý chưa
        if (transaction.Status == "Completed" || transaction.Status == "Failed")
        {
            _logger.LogInformation("Transaction already processed: {TransactionId}", transaction.Id);
            return;
        }

        if (callback.ResultCode == 0)
        {
            // Thanh toán thành công
            _logger.LogInformation("Payment successful for Order #{OrderId}", orderId);

            // Cập nhật transaction
            await _paymentTransactionService.UpdateStatusAsync(transaction.Id, "Completed");

            // Cập nhật order status
            await _orderService.ConfirmOrderAsync(orderId);
        }
        else
        {
            // Thanh toán thất bại
            _logger.LogWarning("Payment failed for Order #{OrderId}: {Message}",
                orderId, callback.Message);

            // Cập nhật transaction
            await _paymentTransactionService.UpdateStatusAsync(transaction.Id, "Failed");

            // Có thể hủy order nếu cần
            // await _orderService.CancelOrderAsync(orderId);
        }
    }

    /// <summary>
    /// Lấy frontend URL từ configuration
    /// </summary>
    private string GetFrontendUrl()
    {
        // TODO: Lấy từ configuration
        return Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
    }
}
