using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Cinema;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Movie;
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
            return StatusCode(500,
                ApiResponseDto<List<CinemaResponseDto>>.ErrorResult("Lỗi khi lấy danh sách rạp chiếu phim"));
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
            return StatusCode(500,
                ApiResponseDto<List<CinemaResponseDto>>.ErrorResult("Lỗi khi lấy danh sách rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Lấy danh sách rạp chiếu phim theo thành phố
    /// </summary>
    [HttpGet("city/{city}")]
    [ProducesResponseType(typeof(List<CinemaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CinemaResponseDto>>> GetByCity(string city,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cinemas = await _cinemaService.GetByCityAsync(city, cancellationToken);
            return Ok(ApiResponseDto<List<CinemaResponseDto>>.SuccessResult(cinemas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinemas by city {City}", city);
            return StatusCode(500,
                ApiResponseDto<List<CinemaResponseDto>>.ErrorResult("Lỗi khi lấy danh sách rạp chiếu phim"));
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
                return NotFound(
                    ApiResponseDto<CinemaResponseDto>.ErrorResult($"Không tìm thấy rạp chiếu phim có ID {id}"));

            return Ok(ApiResponseDto<CinemaResponseDto>.SuccessResult(cinema));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinema by id {CinemaId}", id);
            return StatusCode(500,
                ApiResponseDto<CinemaResponseDto>.ErrorResult("Lỗi khi lấy thông tin rạp chiếu phim"));
        }
    }

    /// <summary>
    /// Tạo rạp chiếu phim mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CinemaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CinemaResponseDto>> Create([FromBody] CinemaCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<CinemaResponseDto>.ErrorResult("Dữ liệu không hợp lệ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var cinema = await _cinemaService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = cinema.Id },
                ApiResponseDto<CinemaResponseDto>.SuccessResult(cinema, "Tạo rạp chiếu phim thành công"));
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
    public async Task<ActionResult<CinemaResponseDto>> Update(Guid id, [FromBody] CinemaUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<CinemaResponseDto>.ErrorResult("Dữ liệu không hợp lệ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var cinema = await _cinemaService.UpdateAsync(id, dto, cancellationToken);
            if (cinema == null)
                return NotFound(
                    ApiResponseDto<CinemaResponseDto>.ErrorResult($"Không tìm thấy rạp chiếu phim có ID {id}"));

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

    /// <summary>
    /// Lấy danh sách phim đang chiếu tại một rạp cụ thể
    /// </summary>
    [HttpGet("{cinemaId}/movies")]
    [ProducesResponseType(typeof(List<MovieResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MovieResponseDto>>> GetCurrentlyShowingMovies(
        Guid cinemaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cinema = await _cinemaService.GetByIdAsync(cinemaId, cancellationToken);
            if (cinema == null)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy rạp có ID {cinemaId}"));

            var movies = await _cinemaService.GetCurrentlyShowingMoviesAsync(cinemaId, cancellationToken);

            var message = movies.Any()
                ? "Lấy danh sách phim đang chiếu thành công"
                : "Rạp hiện không có phim nào đang chiếu";

            return Ok(ApiResponseDto<List<MovieResponseDto>>.SuccessResult(movies, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting showing movies for cinema {CinemaId}", cinemaId);
            return StatusCode(500,
                ApiResponseDto<List<MovieResponseDto>>.ErrorResult("Lỗi khi lấy danh sách phim đang chiếu"));
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết rạp kèm danh sách phim đang chiếu
    /// </summary>
    [HttpGet("{cinemaId}/detail")]
    [ProducesResponseType(typeof(CinemaDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CinemaDetailResponseDto>> GetCinemaDetail(
        Guid cinemaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var detail = await _cinemaService.GetCinemaDetailAsync(cinemaId, cancellationToken);

            if (detail == null)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy rạp có ID {cinemaId}"));

            var message = detail.CurrentlyShowingMovies.Any()
                ? "Lấy thông tin rạp và phim đang chiếu thành công"
                : "Lấy thông tin rạp thành công (hiện chưa có phim đang chiếu)";

            return Ok(ApiResponseDto<CinemaDetailResponseDto>.SuccessResult(detail, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinema detail for {CinemaId}", cinemaId);
            return StatusCode(500,
                ApiResponseDto<CinemaDetailResponseDto>.ErrorResult("Lỗi khi lấy thông tin chi tiết rạp"));
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả rạp kèm phim đang chiếu
    /// </summary>
    [HttpGet("with-movies")]
    [ProducesResponseType(typeof(List<CinemaDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CinemaDetailResponseDto>>> GetAllWithMovies(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cinemas = await _cinemaService.GetAllWithCurrentlyShowingMoviesAsync(cancellationToken);

            return Ok(ApiResponseDto<List<CinemaDetailResponseDto>>.SuccessResult(
                cinemas,
                "Lấy danh sách rạp và phim đang chiếu thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cinemas with movies");
            return StatusCode(500,
                ApiResponseDto<List<CinemaDetailResponseDto>>.ErrorResult("Lỗi khi lấy danh sách rạp"));
        }
    }


    /// <summary>
    /// Lấy tất cả phim kèm lịch chiếu tại rạp (từ giờ trở đi)
    /// </summary>
    [HttpGet("{cinemaId}/movies-with-showtimes")]
    [ProducesResponseType(typeof(CinemaMoviesWithShowtimesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CinemaMoviesWithShowtimesResponseDto>> GetMoviesWithShowtimes(
        Guid cinemaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _cinemaService.GetMoviesWithShowtimesAsync(cinemaId, cancellationToken);

            if (result == null)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy rạp hoặc rạp không hoạt động"));

            return Ok(ApiResponseDto<CinemaMoviesWithShowtimesResponseDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMoviesWithShowtimes for cinema {CinemaId}", cinemaId);
            return StatusCode(500,
                ApiResponseDto<CinemaMoviesWithShowtimesResponseDto>.ErrorResult("Lỗi khi lấy lịch chiếu"));
        }
    }

    /// <summary>
    /// Lấy phim kèm lịch chiếu tại rạp theo ngày cụ thể
    /// </summary>
    [HttpGet("{cinemaId}/movies-with-showtimes/by-date")]
    [ProducesResponseType(typeof(CinemaMoviesWithShowtimesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CinemaMoviesWithShowtimesResponseDto>> GetMoviesWithShowtimesByDate(
        Guid cinemaId,
        [FromQuery] DateTime date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (date == default(DateTime))
                return BadRequest(ApiResponseDto<object>.ErrorResult("Tham số date là bắt buộc"));

            var result = await _cinemaService.GetMoviesWithShowtimesByDateAsync(cinemaId, date, cancellationToken);

            if (result == null)
                return NotFound(
                    ApiResponseDto<object>.ErrorResult("Không tìm thấy rạp hoặc không có lịch chiếu trong ngày này"));

            return Ok(ApiResponseDto<CinemaMoviesWithShowtimesResponseDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMoviesWithShowtimesByDate for cinema {CinemaId}", cinemaId);
            return StatusCode(500,
                ApiResponseDto<CinemaMoviesWithShowtimesResponseDto>.ErrorResult("Lỗi khi lấy lịch chiếu"));
        }
    }
}