using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Showtime;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowtimesController : ControllerBase
{
    private readonly ShowtimeService _showtimeService;
    private readonly ILogger<ShowtimesController> _logger;

    public ShowtimesController(ShowtimeService showtimeService, ILogger<ShowtimesController> logger)
    {
        _showtimeService = showtimeService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách lịch chiếu theo phim
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(List<ShowtimeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShowtimeResponseDto>>> GetByMovieId(Guid movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            var showtimes = await _showtimeService.GetByMovieIdAsync(movieId, cancellationToken);
            return Ok(ApiResponseDto<List<ShowtimeResponseDto>>.SuccessResult(showtimes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting showtimes by movie {MovieId}", movieId);
            return StatusCode(500, ApiResponseDto<List<ShowtimeResponseDto>>.ErrorResult("Lỗi khi lấy danh sách lịch chiếu"));
        }
    }

    /// <summary>
    /// Lấy danh sách lịch chiếu theo ngày
    /// </summary>
    [HttpGet("date/{date}")]
    [ProducesResponseType(typeof(List<ShowtimeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShowtimeResponseDto>>> GetByDate(DateTime date, CancellationToken cancellationToken = default)
    {
        try
        {
            var showtimes = await _showtimeService.GetByDateAsync(date, cancellationToken);
            return Ok(ApiResponseDto<List<ShowtimeResponseDto>>.SuccessResult(showtimes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting showtimes by date {Date}", date);
            return StatusCode(500, ApiResponseDto<List<ShowtimeResponseDto>>.ErrorResult("Lỗi khi lấy danh sách lịch chiếu"));
        }
    }

    /// <summary>
    /// Lấy danh sách lịch chiếu theo phim và ngày
    /// </summary>
    [HttpGet("movie/{movieId}/date/{date}")]
    [ProducesResponseType(typeof(List<ShowtimeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShowtimeResponseDto>>> GetByMovieAndDate(Guid movieId, DateTime date, CancellationToken cancellationToken = default)
    {
        try
        {
            var showtimes = await _showtimeService.GetByMovieAndDateAsync(movieId, date, cancellationToken);
            return Ok(ApiResponseDto<List<ShowtimeResponseDto>>.SuccessResult(showtimes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting showtimes by movie and date");
            return StatusCode(500, ApiResponseDto<List<ShowtimeResponseDto>>.ErrorResult("Lỗi khi lấy danh sách lịch chiếu"));
        }
    }

    /// <summary>
    /// Lấy thông tin lịch chiếu theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShowtimeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShowtimeResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var showtime = await _showtimeService.GetByIdAsync(id, cancellationToken);
            if (showtime == null)
                return NotFound(ApiResponseDto<ShowtimeResponseDto>.ErrorResult($"Không tìm thấy lịch chiếu có ID {id}"));

            return Ok(ApiResponseDto<ShowtimeResponseDto>.SuccessResult(showtime));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting showtime by id {ShowtimeId}", id);
            return StatusCode(500, ApiResponseDto<ShowtimeResponseDto>.ErrorResult("Lỗi khi lấy thông tin lịch chiếu"));
        }
    }

    /// <summary>
    /// Tạo lịch chiếu mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShowtimeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShowtimeResponseDto>> Create([FromBody] ShowtimeCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ShowtimeResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var showtime = await _showtimeService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, ApiResponseDto<ShowtimeResponseDto>.SuccessResult(showtime, "Tạo lịch chiếu thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ShowtimeResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating showtime");
            return StatusCode(500, ApiResponseDto<ShowtimeResponseDto>.ErrorResult("Lỗi khi tạo lịch chiếu"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin lịch chiếu
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ShowtimeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShowtimeResponseDto>> Update(Guid id, [FromBody] ShowtimeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ShowtimeResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var showtime = await _showtimeService.UpdateAsync(id, dto, cancellationToken);
            if (showtime == null)
                return NotFound(ApiResponseDto<ShowtimeResponseDto>.ErrorResult($"Không tìm thấy lịch chiếu có ID {id}"));

            return Ok(ApiResponseDto<ShowtimeResponseDto>.SuccessResult(showtime, "Cập nhật lịch chiếu thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ShowtimeResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating showtime {ShowtimeId}", id);
            return StatusCode(500, ApiResponseDto<ShowtimeResponseDto>.ErrorResult("Lỗi khi cập nhật lịch chiếu"));
        }
    }

    /// <summary>
    /// Xóa lịch chiếu
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _showtimeService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy lịch chiếu có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting showtime {ShowtimeId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa lịch chiếu"));
        }
    }
}

