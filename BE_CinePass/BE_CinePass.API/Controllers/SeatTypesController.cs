using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.SeatType;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatTypesController : ControllerBase
{
    private readonly SeatTypeService _seatTypeService;
    private readonly ILogger<SeatTypesController> _logger;

    public SeatTypesController(SeatTypeService seatTypeService, ILogger<SeatTypesController> logger)
    {
        _seatTypeService = seatTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả loại ghế
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SeatTypeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SeatTypeResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var seatTypes = await _seatTypeService.GetAllAsync(cancellationToken);
            return Ok(ApiResponseDto<List<SeatTypeResponseDto>>.SuccessResult(seatTypes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all seat types");
            return StatusCode(500, ApiResponseDto<List<SeatTypeResponseDto>>.ErrorResult("Lỗi khi lấy danh sách loại ghế"));
        }
    }

    /// <summary>
    /// Lấy thông tin loại ghế theo mã
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(SeatTypeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatTypeResponseDto>> GetByCode(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var seatType = await _seatTypeService.GetByCodeAsync(code, cancellationToken);
            if (seatType == null)
                return NotFound(ApiResponseDto<SeatTypeResponseDto>.ErrorResult($"Không tìm thấy loại ghế có mã {code}"));

            return Ok(ApiResponseDto<SeatTypeResponseDto>.SuccessResult(seatType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seat type by code {Code}", code);
            return StatusCode(500, ApiResponseDto<SeatTypeResponseDto>.ErrorResult("Lỗi khi lấy thông tin loại ghế"));
        }
    }

    /// <summary>
    /// Tạo loại ghế mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SeatTypeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeatTypeResponseDto>> Create([FromBody] SeatTypeCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<SeatTypeResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var seatType = await _seatTypeService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetByCode), new { code = seatType.Code }, ApiResponseDto<SeatTypeResponseDto>.SuccessResult(seatType, "Tạo loại ghế thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<SeatTypeResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating seat type");
            return StatusCode(500, ApiResponseDto<SeatTypeResponseDto>.ErrorResult("Lỗi khi tạo loại ghế"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin loại ghế
    /// </summary>
    [HttpPut("{code}")]
    [ProducesResponseType(typeof(SeatTypeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeatTypeResponseDto>> Update(string code, [FromBody] SeatTypeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<SeatTypeResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var seatType = await _seatTypeService.UpdateAsync(code, dto, cancellationToken);
            if (seatType == null)
                return NotFound(ApiResponseDto<SeatTypeResponseDto>.ErrorResult($"Không tìm thấy loại ghế có mã {code}"));

            return Ok(ApiResponseDto<SeatTypeResponseDto>.SuccessResult(seatType, "Cập nhật loại ghế thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating seat type {Code}", code);
            return StatusCode(500, ApiResponseDto<SeatTypeResponseDto>.ErrorResult("Lỗi khi cập nhật loại ghế"));
        }
    }

    /// <summary>
    /// Xóa loại ghế
    /// </summary>
    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _seatTypeService.DeleteAsync(code, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy loại ghế có mã {code}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting seat type {Code}", code);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa loại ghế"));
        }
    }
}

