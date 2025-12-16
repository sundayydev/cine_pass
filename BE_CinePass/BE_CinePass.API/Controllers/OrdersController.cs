using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Order;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin đơn hàng theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id, cancellationToken);
            if (order == null)
                return NotFound(ApiResponseDto<OrderResponseDto>.ErrorResult($"Không tìm thấy đơn hàng có ID {id}"));

            return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by id {OrderId}", id);
            return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("Lỗi khi lấy thông tin đơn hàng"));
        }
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng theo ID (bao gồm tickets và products)
    /// </summary>
    [HttpGet("{id}/detail")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDto>> GetDetailById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderService.GetDetailByIdAsync(id, cancellationToken);
            if (order == null)
                return NotFound(ApiResponseDto<OrderDetailDto>.ErrorResult($"Không tìm thấy đơn hàng có ID {id}"));

            return Ok(ApiResponseDto<OrderDetailDto>.SuccessResult(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order detail by id {OrderId}", id);
            return StatusCode(500, ApiResponseDto<OrderDetailDto>.ErrorResult("Lỗi khi lấy chi tiết đơn hàng"));
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng theo người dùng
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderResponseDto>>> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetByUserIdAsync(userId, cancellationToken);
            return Ok(ApiResponseDto<List<OrderResponseDto>>.SuccessResult(orders));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by user {UserId}", userId);
            return StatusCode(500, ApiResponseDto<List<OrderResponseDto>>.ErrorResult("Lỗi khi lấy danh sách đơn hàng"));
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng theo trạng thái
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderResponseDto>>> GetByStatus(OrderStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetByStatusAsync(status, cancellationToken);
            return Ok(ApiResponseDto<List<OrderResponseDto>>.SuccessResult(orders));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status {Status}", status);
            return StatusCode(500, ApiResponseDto<List<OrderResponseDto>>.ErrorResult("Lỗi khi lấy danh sách đơn hàng"));
        }
    }

    /// <summary>
    /// Tạo đơn hàng mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var order = await _orderService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, ApiResponseDto<OrderResponseDto>.SuccessResult(order, "Tạo đơn hàng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("Lỗi khi tạo đơn hàng"));
        }
    }

    /// <summary>
    /// Cập nhật đơn hàng
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponseDto>> Update(Guid id, [FromBody] OrderUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var order = await _orderService.UpdateAsync(id, dto, cancellationToken);
            if (order == null)
                return NotFound(ApiResponseDto<OrderResponseDto>.ErrorResult($"Không tìm thấy đơn hàng có ID {id}"));

            return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(order, "Cập nhật đơn hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("Lỗi khi cập nhật đơn hàng"));
        }
    }

    /// <summary>
    /// Xác nhận đơn hàng (sau khi thanh toán thành công)
    /// </summary>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmOrder(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orderService.ConfirmOrderAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy đơn hàng có ID {id}"));

            return Ok(ApiResponseDto<object>.SuccessResult("Xác nhận đơn hàng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order {OrderId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xác nhận đơn hàng"));
        }
    }

    /// <summary>
    /// Hủy đơn hàng
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy đơn hàng có ID {id}"));

            return Ok(ApiResponseDto<object>.SuccessResult("Hủy đơn hàng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi hủy đơn hàng"));
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng đã hết hạn
    /// </summary>
    [HttpGet("expired")]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderResponseDto>>> GetExpired(CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetExpiredOrdersAsync(cancellationToken);
            return Ok(ApiResponseDto<List<OrderResponseDto>>.SuccessResult(orders));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired orders");
            return StatusCode(500, ApiResponseDto<List<OrderResponseDto>>.ErrorResult("Lỗi khi lấy danh sách đơn hàng hết hạn"));
        }
    }
}

