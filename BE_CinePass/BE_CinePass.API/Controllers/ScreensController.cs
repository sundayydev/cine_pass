using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Screen;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreensController : ControllerBase
{
    private readonly ScreenService _screenService;
    private readonly ILogger<ScreensController> _logger;

    public ScreensController(ScreenService screenService, ILogger<ScreensController> logger)
    {
        _screenService = screenService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách màn hình theo rạp chiếu phim
    /// </summary>
    [HttpGet("cinema/{cinemaId}")]
    [ProducesResponseType(typeof(List<ScreenResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ScreenResponseDto>>> GetByCinemaId(Guid cinemaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var screens = await _screenService.GetByCinemaIdAsync(cinemaId, cancellationToken);
            return Ok(ApiResponseDto<List<ScreenResponseDto>>.SuccessResult(screens));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screens by cinema {CinemaId}", cinemaId);
            return StatusCode(500, ApiResponseDto<List<ScreenResponseDto>>.ErrorResult("Lỗi khi lấy danh sách màn hình"));
        }
    }

    /// <summary>
    /// Lấy thông tin màn hình theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScreenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScreenResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var screen = await _screenService.GetByIdAsync(id, cancellationToken);
            if (screen == null)
                return NotFound(ApiResponseDto<ScreenResponseDto>.ErrorResult($"Không tìm thấy màn hình có ID {id}"));

            return Ok(ApiResponseDto<ScreenResponseDto>.SuccessResult(screen));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screen by id {ScreenId}", id);
            return StatusCode(500, ApiResponseDto<ScreenResponseDto>.ErrorResult("Lỗi khi lấy thông tin màn hình"));
        }
    }

    /// <summary>
    /// Tạo màn hình mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ScreenResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScreenResponseDto>> Create([FromBody] ScreenCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ScreenResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var screen = await _screenService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = screen.Id }, ApiResponseDto<ScreenResponseDto>.SuccessResult(screen, "Tạo màn hình thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ScreenResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating screen");
            return StatusCode(500, ApiResponseDto<ScreenResponseDto>.ErrorResult("Lỗi khi tạo màn hình"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin màn hình
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ScreenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScreenResponseDto>> Update(Guid id, [FromBody] ScreenUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ScreenResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var screen = await _screenService.UpdateAsync(id, dto, cancellationToken);
            if (screen == null)
                return NotFound(ApiResponseDto<ScreenResponseDto>.ErrorResult($"Không tìm thấy màn hình có ID {id}"));

            return Ok(ApiResponseDto<ScreenResponseDto>.SuccessResult(screen, "Cập nhật màn hình thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ScreenResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating screen {ScreenId}", id);
            return StatusCode(500, ApiResponseDto<ScreenResponseDto>.ErrorResult("Lỗi khi cập nhật màn hình"));
        }
    }

    /// <summary>
    /// Xóa màn hình
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _screenService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy màn hình có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting screen {ScreenId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa màn hình"));
        }
    }
}

