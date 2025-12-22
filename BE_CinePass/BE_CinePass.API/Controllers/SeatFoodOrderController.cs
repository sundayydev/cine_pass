using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Order;
using BE_CinePass.Shared.DTOs.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

/// <summary>
/// Controller xử lý order đồ ăn/nước uống từ ghế ngồi trong rạp qua QR code
/// Cho phép khách hàng quét mã QR trên ghế để order đồ ăn/nước uống giao đến ghế
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeatFoodOrderController : ControllerBase
{
    private readonly SeatFoodOrderService _seatFoodOrderService;
    private readonly ProductService _productService;
    private readonly ILogger<SeatFoodOrderController> _logger;

    public SeatFoodOrderController(
        SeatFoodOrderService seatFoodOrderService,
        ProductService productService,
        ILogger<SeatFoodOrderController> logger)
    {
        _seatFoodOrderService = seatFoodOrderService;
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Kiểm tra thông tin ghế từ mã QR
    /// Sử dụng để validate mã QR và hiển thị thông tin phim đang chiếu
    /// </summary>
    /// <param name="seatQrCode">Mã QR ordering của ghế (6 ký tự)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Thông tin ghế, suất chiếu đang diễn ra và khả năng order</returns>
    [HttpGet("seat-info/{seatQrCode}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SeatInfoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeatInfoResponseDto>> GetSeatInfo(
        string seatQrCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(seatQrCode))
                return BadRequest(ApiResponseDto<SeatInfoResponseDto>.ErrorResult("Mã QR ghế không được để trống"));

            var seatInfo = await _seatFoodOrderService.GetSeatInfoAsync(seatQrCode.Trim(), cancellationToken);

            var message = seatInfo.CanOrderFood
                ? "Ghế hợp lệ, có thể order đồ ăn"
                : seatInfo.ErrorMessage ?? "Không thể order đồ ăn";

            return Ok(ApiResponseDto<SeatInfoResponseDto>.SuccessResult(seatInfo, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seat info for QR code: {QrCode}", seatQrCode);
            return StatusCode(500, ApiResponseDto<SeatInfoResponseDto>.ErrorResult("Lỗi khi kiểm tra thông tin ghế"));
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm có thể order (đồ ăn, nước uống đang hoạt động)
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Danh sách sản phẩm</returns>
    [HttpGet("products")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponseDto>>> GetAvailableProducts(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _productService.GetActiveProductsAsync(cancellationToken);
            return Ok(ApiResponseDto<List<ProductResponseDto>>.SuccessResult(products, $"Tìm thấy {products.Count} sản phẩm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available products");
            return StatusCode(500, ApiResponseDto<List<ProductResponseDto>>.ErrorResult("Lỗi khi lấy danh sách sản phẩm"));
        }
    }

    /// <summary>
    /// Tạo đơn hàng đồ ăn/nước uống từ ghế ngồi
    /// </summary>
    /// <param name="dto">Thông tin đơn hàng</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Chi tiết đơn hàng và thông tin thanh toán</returns>
    [HttpPost("order")]
    [AllowAnonymous] // Cho phép khách không đăng nhập cũng order được
    [ProducesResponseType(typeof(SeatFoodOrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeatFoodOrderResponseDto>> CreateOrder(
        [FromBody] SeatFoodOrderCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<SeatFoodOrderResponseDto>.ErrorResult(
                    "Dữ liệu không hợp lệ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            _logger.LogInformation(
                "Creating seat food order. QR: {QrCode}, Items: {ItemCount}, PaymentMethod: {PaymentMethod}",
                dto.SeatQrCode, dto.Items.Count, dto.PaymentMethod);

            var result = await _seatFoodOrderService.CreateOrderAsync(dto, cancellationToken);

            _logger.LogInformation(
                "Seat food order created successfully. OrderId: {OrderId}, Total: {Total} VND",
                result.OrderId, result.TotalAmount);

            return CreatedAtAction(
                nameof(GetOrderStatus),
                new { orderId = result.OrderId },
                ApiResponseDto<SeatFoodOrderResponseDto>.SuccessResult(result, result.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create seat food order: {Message}", ex.Message);
            return BadRequest(ApiResponseDto<SeatFoodOrderResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating seat food order");
            return StatusCode(500, ApiResponseDto<SeatFoodOrderResponseDto>.ErrorResult("Lỗi khi tạo đơn hàng"));
        }
    }

    /// <summary>
    /// Lấy trạng thái đơn hàng đồ ăn
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Trạng thái đơn hàng</returns>
    [HttpGet("order/{orderId}/status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SeatFoodOrderStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatFoodOrderStatusDto>> GetOrderStatus(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _seatFoodOrderService.GetOrderStatusAsync(orderId, cancellationToken);

            if (status == null)
                return NotFound(ApiResponseDto<SeatFoodOrderStatusDto>.ErrorResult("Không tìm thấy đơn hàng"));

            return Ok(ApiResponseDto<SeatFoodOrderStatusDto>.SuccessResult(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order status for OrderId: {OrderId}", orderId);
            return StatusCode(500, ApiResponseDto<SeatFoodOrderStatusDto>.ErrorResult("Lỗi khi lấy trạng thái đơn hàng"));
        }
    }

    /// <summary>
    /// Hủy đơn hàng đồ ăn (chỉ khi chưa thanh toán)
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Kết quả hủy</returns>
    [HttpPost("order/{orderId}/cancel")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _seatFoodOrderService.CancelOrderAsync(orderId, cancellationToken);

            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy đơn hàng"));

            _logger.LogInformation("Seat food order cancelled. OrderId: {OrderId}", orderId);

            return Ok(ApiResponseDto<object>.SuccessResult("Đã hủy đơn hàng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi hủy đơn hàng"));
        }
    }

    /// <summary>
    /// Xác nhận thanh toán tiền mặt (dành cho nhân viên)
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Chi tiết đơn hàng đã xác nhận</returns>
    [HttpPost("order/{orderId}/confirm-cash")]
    // [Authorize(Roles = "Staff,Admin")] // Enable when auth is ready
    [ProducesResponseType(typeof(SeatFoodOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatFoodOrderResponseDto>> ConfirmCashPayment(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _seatFoodOrderService.ConfirmCashPaymentAsync(orderId, cancellationToken);

            _logger.LogInformation(
                "Cash payment confirmed for seat food order. OrderId: {OrderId}",
                orderId);

            return Ok(ApiResponseDto<SeatFoodOrderResponseDto>.SuccessResult(result, result.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<SeatFoodOrderResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming cash payment for order {OrderId}", orderId);
            return StatusCode(500, ApiResponseDto<SeatFoodOrderResponseDto>.ErrorResult("Lỗi khi xác nhận thanh toán"));
        }
    }

    /// <summary>
    /// Lấy lịch sử đơn hàng của ghế trong ngày
    /// </summary>
    /// <param name="seatQrCode">Mã QR ghế</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Danh sách đơn hàng</returns>
    [HttpGet("seat/{seatQrCode}/orders")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SeatFoodOrderStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SeatFoodOrderStatusDto>>> GetOrdersBySeat(
        string seatQrCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _seatFoodOrderService.GetOrdersBySeatAsync(seatQrCode, cancellationToken);
            return Ok(ApiResponseDto<List<SeatFoodOrderStatusDto>>.SuccessResult(
                orders,
                orders.Count > 0 ? $"Tìm thấy {orders.Count} đơn hàng" : "Không có đơn hàng nào"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for seat QR: {QrCode}", seatQrCode);
            return StatusCode(500, ApiResponseDto<List<SeatFoodOrderStatusDto>>.ErrorResult("Lỗi khi lấy danh sách đơn hàng"));
        }
    }
}
