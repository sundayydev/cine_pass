using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Cinema;
using BE_CinePass.Shared.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CinemasController : ControllerBase
{
    private readonly CinemaService _cinemaService;
    private readonly ILogger<CinemasController> _logger;

    public CinemasController(CinemaService cinemaService, ILogger<CinemasController> logger)
    {
        _cinemaService = cinemaService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả rạp chiếu phim
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CinemaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CinemaResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var cinemas = await _cinemaService.GetAllAsync(cancellationToken);
            return Ok(ApiResponseDto<List<CinemaResponseDto>>.SuccessResult(cinemas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cinemas");
            return StatusCode(500, ApiResponseDto<List<CinemaResponseDto>>.ErrorResult("Lỗi khi lấy danh sách rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Lấy danh sách rạp chiếu phim đang hoạt động
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<CinemaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CinemaResponseDto>>> GetActive(CancellationToken cancellationToken = default)
    {
        try
        {
            var cinemas = await _cinemaService.GetActiveCinemasAsync(cancellationToken);
            return Ok(ApiResponseDto<List<CinemaResponseDto>>.SuccessResult(cinemas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active cinemas");
            return StatusCode(500, ApiResponseDto<List<CinemaResponseDto>>.ErrorResult("Lỗi khi lấy danh sách rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Lấy danh sách rạp chiếu phim theo thành phố
    /// </summary>
    [HttpGet("city/{city}")]
    [ProducesResponseType(typeof(List<CinemaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CinemaResponseDto>>> GetByCity(string city, CancellationToken cancellationToken = default)
    {
        try
        {
            var cinemas = await _cinemaService.GetByCityAsync(city, cancellationToken);
            return Ok(ApiResponseDto<List<CinemaResponseDto>>.SuccessResult(cinemas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinemas by city {City}", city);
            return StatusCode(500, ApiResponseDto<List<CinemaResponseDto>>.ErrorResult("Lỗi khi lấy danh sách rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Lấy thông tin rạp chiếu phim theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CinemaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CinemaResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cinema = await _cinemaService.GetByIdAsync(id, cancellationToken);
            if (cinema == null)
                return NotFound(ApiResponseDto<CinemaResponseDto>.ErrorResult($"Không tìm thấy rạp chiếu phim có ID {id}"));

            return Ok(ApiResponseDto<CinemaResponseDto>.SuccessResult(cinema));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinema by id {CinemaId}", id);
            return StatusCode(500, ApiResponseDto<CinemaResponseDto>.ErrorResult("Lỗi khi lấy thông tin rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Tạo rạp chiếu phim mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CinemaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CinemaResponseDto>> Create([FromBody] CinemaCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<CinemaResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var cinema = await _cinemaService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = cinema.Id }, ApiResponseDto<CinemaResponseDto>.SuccessResult(cinema, "Tạo rạp chiếu phim thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cinema");
            return StatusCode(500, ApiResponseDto<CinemaResponseDto>.ErrorResult("Lỗi khi tạo rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin rạp chiếu phim
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CinemaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CinemaResponseDto>> Update(Guid id, [FromBody] CinemaUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<CinemaResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var cinema = await _cinemaService.UpdateAsync(id, dto, cancellationToken);
            if (cinema == null)
                return NotFound(ApiResponseDto<CinemaResponseDto>.ErrorResult($"Không tìm thấy rạp chiếu phim có ID {id}"));

            return Ok(ApiResponseDto<CinemaResponseDto>.SuccessResult(cinema, "Cập nhật rạp chiếu phim thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cinema {CinemaId}", id);
            return StatusCode(500, ApiResponseDto<CinemaResponseDto>.ErrorResult("Lỗi khi cập nhật rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Xóa rạp chiếu phim
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _cinemaService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy rạp chiếu phim có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cinema {CinemaId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa rạp chiếu phim"));
        }
    }
}

