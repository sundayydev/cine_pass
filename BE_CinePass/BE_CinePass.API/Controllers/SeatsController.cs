using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Seat;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatsController : ControllerBase
{
    private readonly SeatService _seatService;
    private readonly ILogger<SeatsController> _logger;

    public SeatsController(SeatService seatService, ILogger<SeatsController> logger)
    {
        _seatService = seatService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách ghế theo màn hình
    /// </summary>
    [HttpGet("screen/{screenId}")]
    [ProducesResponseType(typeof(List<SeatResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SeatResponseDto>>> GetByScreenId(Guid screenId, CancellationToken cancellationToken = default)
    {
        try
        {
            var seats = await _seatService.GetByScreenIdAsync(screenId, cancellationToken);
            return Ok(ApiResponseDto<List<SeatResponseDto>>.SuccessResult(seats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seats by screen {ScreenId}", screenId);
            return StatusCode(500, ApiResponseDto<List<SeatResponseDto>>.ErrorResult("Lỗi khi lấy danh sách ghế"));
        }
    }

    /// <summary>
    /// Lấy danh sách ghế đang hoạt động theo màn hình
    /// </summary>
    [HttpGet("screen/{screenId}/active")]
    [ProducesResponseType(typeof(List<SeatResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SeatResponseDto>>> GetActiveByScreenId(Guid screenId, CancellationToken cancellationToken = default)
    {
        try
        {
            var seats = await _seatService.GetActiveSeatsByScreenIdAsync(screenId, cancellationToken);
            return Ok(ApiResponseDto<List<SeatResponseDto>>.SuccessResult(seats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active seats by screen {ScreenId}", screenId);
            return StatusCode(500, ApiResponseDto<List<SeatResponseDto>>.ErrorResult("Lỗi khi lấy danh sách ghế"));
        }
    }

    /// <summary>
    /// Lấy thông tin ghế theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SeatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var seat = await _seatService.GetByIdAsync(id, cancellationToken);
            if (seat == null)
                return NotFound(ApiResponseDto<SeatResponseDto>.ErrorResult($"Không tìm thấy ghế có ID {id}"));

            return Ok(ApiResponseDto<SeatResponseDto>.SuccessResult(seat));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seat by id {SeatId}", id);
            return StatusCode(500, ApiResponseDto<SeatResponseDto>.ErrorResult("Lỗi khi lấy thông tin ghế"));
        }
    }

    /// <summary>
    /// Kiểm tra ghế có còn trống không
    /// </summary>
    [HttpGet("{seatId}/available")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsSeatAvailable(Guid seatId, [FromQuery] Guid showtimeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = await _seatService.IsSeatAvailableAsync(seatId, showtimeId, cancellationToken);
            return Ok(ApiResponseDto<bool>.SuccessResult(isAvailable));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking seat availability");
            return StatusCode(500, ApiResponseDto<bool>.ErrorResult("Lỗi khi kiểm tra ghế"));
        }
    }

    /// <summary>
    /// Tạo ghế mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SeatResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeatResponseDto>> Create([FromBody] SeatCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<SeatResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var seat = await _seatService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = seat.Id }, ApiResponseDto<SeatResponseDto>.SuccessResult(seat, "Tạo ghế thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<SeatResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating seat");
            return StatusCode(500, ApiResponseDto<SeatResponseDto>.ErrorResult("Lỗi khi tạo ghế"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin ghế
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SeatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeatResponseDto>> Update(Guid id, [FromBody] SeatUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<SeatResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var seat = await _seatService.UpdateAsync(id, dto, cancellationToken);
            if (seat == null)
                return NotFound(ApiResponseDto<SeatResponseDto>.ErrorResult($"Không tìm thấy ghế có ID {id}"));

            return Ok(ApiResponseDto<SeatResponseDto>.SuccessResult(seat, "Cập nhật ghế thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<SeatResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating seat {SeatId}", id);
            return StatusCode(500, ApiResponseDto<SeatResponseDto>.ErrorResult("Lỗi khi cập nhật ghế"));
        }
    }

    /// <summary>
    /// Tự động tạo ghế theo cấu hình
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(List<SeatResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SeatResponseDto>>> GenerateSeats([FromBody] SeatGenerateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<List<SeatResponseDto>>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var seats = await _seatService.GenerateSeatsAsync(dto.ScreenId, dto.Rows, dto.SeatsPerRow, dto.DefaultSeatTypeCode, cancellationToken);
            return CreatedAtAction(nameof(GetByScreenId), new { screenId = dto.ScreenId }, ApiResponseDto<List<SeatResponseDto>>.SuccessResult(seats, $"Đã tạo {seats.Count} ghế thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<List<SeatResponseDto>>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating seats");
            return StatusCode(500, ApiResponseDto<List<SeatResponseDto>>.ErrorResult("Lỗi khi tạo ghế tự động"));
        }
    }

    /// <summary>
    /// Xóa ghế
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _seatService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy ghế có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting seat {SeatId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa ghế"));
        }
    }
}

