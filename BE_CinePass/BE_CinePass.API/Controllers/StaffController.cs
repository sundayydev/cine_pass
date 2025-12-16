using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Order;
using BE_CinePass.Shared.DTOs.ETicket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Staff,Admin")] // Uncomment when authentication is ready
public class StaffController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ETicketService _eTicketService;
    private readonly ILogger<StaffController> _logger;

    public StaffController(
        OrderService orderService,
        ETicketService eTicketService,
        ILogger<StaffController> logger)
    {
        _orderService = orderService;
        _eTicketService = eTicketService;
        _logger = logger;
    }

    /// <summary>
    /// Tìm kiếm đơn hàng theo số điện thoại khách hàng (hỗ trợ khách quên vé/mất điện thoại)
    /// </summary>
    /// <param name="phone">Số điện thoại khách hàng</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Danh sách đơn hàng đã xác nhận của khách hàng</returns>
    [HttpGet("orders/search")]
    [ProducesResponseType(typeof(List<OrderSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<OrderSearchResultDto>>> SearchOrdersByPhone(
        [FromQuery] string phone,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(ApiResponseDto<List<OrderSearchResultDto>>.ErrorResult("Số điện thoại không được để trống"));

            // Clean phone number (remove spaces, dashes, etc.)
            phone = phone.Trim().Replace(" ", "").Replace("-", "");

            var orders = await _orderService.SearchByPhoneAsync(phone, cancellationToken);

            if (orders.Count == 0)
            {
                return Ok(ApiResponseDto<List<OrderSearchResultDto>>.SuccessResult(
                    orders,
                    $"Không tìm thấy đơn hàng nào với số điện thoại {phone}"));
            }

            return Ok(ApiResponseDto<List<OrderSearchResultDto>>.SuccessResult(
                orders,
                $"Tìm thấy {orders.Count} đơn hàng"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching orders by phone {Phone}", phone);
            return StatusCode(500, ApiResponseDto<List<OrderSearchResultDto>>.ErrorResult("Lỗi khi tìm kiếm đơn hàng"));
        }
    }

    /// <summary>
    /// In lại vé cho khách hàng
    /// </summary>
    /// <param name="id">ID đơn hàng</param>
    /// <param name="dto">Thông tin in vé (lý do, ghi chú)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Chi tiết đơn hàng với thông tin vé để in</returns>
    [HttpPost("orders/{id}/print")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDetailDto>> PrintTickets(
        Guid id,
        [FromBody] PrintTicketDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orderDetail = await _orderService.GetForPrintAsync(id, cancellationToken);

            if (orderDetail == null)
                return NotFound(ApiResponseDto<OrderDetailDto>.ErrorResult($"Không tìm thấy đơn hàng có ID {id}"));

            // Log the print action for audit trail
            _logger.LogInformation(
                "Ticket reprint requested for order {OrderId}. Reason: {Reason}. Staff Note: {Note}",
                id, dto.PrintReason, dto.StaffNote ?? "None");

            return Ok(ApiResponseDto<OrderDetailDto>.SuccessResult(
                orderDetail,
                "Lấy thông tin vé thành công. Vui lòng in vé cho khách hàng."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<OrderDetailDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing tickets for order {OrderId}", id);
            return StatusCode(500, ApiResponseDto<OrderDetailDto>.ErrorResult("Lỗi khi lấy thông tin in vé"));
        }
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng (cho nhân viên)
    /// </summary>
    /// <param name="id">ID đơn hàng</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Chi tiết đầy đủ của đơn hàng</returns>
    [HttpGet("orders/{id}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orderDetail = await _orderService.GetDetailByIdAsync(id, cancellationToken);

            if (orderDetail == null)
                return NotFound(ApiResponseDto<OrderDetailDto>.ErrorResult($"Không tìm thấy đơn hàng có ID {id}"));

            return Ok(ApiResponseDto<OrderDetailDto>.SuccessResult(orderDetail));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order detail {OrderId}", id);
            return StatusCode(500, ApiResponseDto<OrderDetailDto>.ErrorResult("Lỗi khi lấy thông tin đơn hàng"));
        }
    }

    /// <summary>
    /// Quét QR Code vé để verify và check-in
    /// </summary>
    /// <param name="dto">Dữ liệu QR code từ e-ticket</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Kết quả xác thực: Hợp lệ / Không hợp lệ / Đã dùng rồi</returns>
    [HttpPost("tickets/verify")]
    [ProducesResponseType(typeof(BE_CinePass.Shared.DTOs.ETicket.TicketVerificationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BE_CinePass.Shared.DTOs.ETicket.TicketVerificationResultDto>> VerifyTicket(
        [FromBody] BE_CinePass.Shared.DTOs.ETicket.VerifyTicketDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.QrData))
                return BadRequest(ApiResponseDto<BE_CinePass.Shared.DTOs.ETicket.TicketVerificationResultDto>
                    .ErrorResult("Dữ liệu QR code không được để trống"));

            var result = await _eTicketService.VerifyAndUseTicketAsync(dto.QrData, cancellationToken);

            // Log the verification attempt
            if (result.IsValid)
            {
                _logger.LogInformation(
                    "Ticket verified successfully. QR: {QrData}, Movie: {Movie}, Seat: {Seat}",
                    dto.QrData.Substring(0, Math.Min(20, dto.QrData.Length)),
                    result.TicketDetail?.OrderTicket?.Showtime?.Movie?.Title ?? "N/A",
                    result.TicketDetail?.OrderTicket?.Seat?.SeatCode ?? "N/A");
            }
            else
            {
                _logger.LogWarning(
                    "Ticket verification failed. QR: {QrData}, Status: {Status}, Message: {Message}",
                    dto.QrData.Substring(0, Math.Min(20, dto.QrData.Length)),
                    result.Status,
                    result.Message);
            }

            return Ok(ApiResponseDto<BE_CinePass.Shared.DTOs.ETicket.TicketVerificationResultDto>
                .SuccessResult(result, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying ticket");
            return StatusCode(500, ApiResponseDto<BE_CinePass.Shared.DTOs.ETicket.TicketVerificationResultDto>
                .ErrorResult("Lỗi khi xác thực vé"));
        }
    }

    /// <summary>
    /// Tạo đơn hàng POS (Point of Sale) và thanh toán tiền mặt ngay lập tức
    /// </summary>
    /// <param name="dto">Thông tin đơn hàng và thanh toán</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Dữ liệu vé để in bằng máy in nhiệt</returns>
    [HttpPost("orders/pos-create")]
    [ProducesResponseType(typeof(PosOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PosOrderResponseDto>> CreatePosOrder(
        [FromBody] PosOrderCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orderService.CreatePosOrderAsync(dto, cancellationToken);

            _logger.LogInformation(
                "POS order created successfully. OrderId: {OrderId}, Customer: {CustomerName}, Phone: {Phone}, Total: {Total} VND, Cash: {Cash} VND, Change: {Change} VND",
                result.OrderDetail.Id,
                dto.CustomerName,
                dto.CustomerPhone,
                result.PaymentInfo.TotalAmount,
                result.PaymentInfo.CashReceived,
                result.PaymentInfo.ChangeAmount);

            return Ok(ApiResponseDto<PosOrderResponseDto>.SuccessResult(
                result,
                $"Đơn hàng đã được tạo thành công. Tiền thừa: {result.PaymentInfo.ChangeAmount:N0} VND"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "POS order creation failed: {Message}", ex.Message);
            return BadRequest(ApiResponseDto<PosOrderResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating POS order");
            return StatusCode(500, ApiResponseDto<PosOrderResponseDto>.ErrorResult("Lỗi khi tạo đơn hàng"));
        }
    }
}
