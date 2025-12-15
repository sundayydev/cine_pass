using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Movie;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly MovieService _movieService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(MovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả phim
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MovieResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var movies = await _movieService.GetAllAsync(cancellationToken);
            return Ok(ApiResponseDto<List<MovieResponseDto>>.SuccessResult(movies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all movies");
            return StatusCode(500, ApiResponseDto<List<MovieResponseDto>>.ErrorResult("Lỗi khi lấy danh sách phim"));
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết phim theo ID (Info, Trailer, Actors, Reviews)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MovieDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var movie = await _movieService.GetByIdAsync(id, cancellationToken);
            if (movie == null)
                return NotFound(ApiResponseDto<MovieDetailResponseDto>.ErrorResult($"Không tìm thấy phim có ID {id}"));

            return Ok(ApiResponseDto<MovieDetailResponseDto>.SuccessResult(movie));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie by id {MovieId}", id);
            return StatusCode(500, ApiResponseDto<MovieDetailResponseDto>.ErrorResult("Lỗi khi lấy thông tin phim"));
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết phim theo slug (Info, Trailer, Actors, Reviews)
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(MovieDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailResponseDto>> GetBySlug(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var movie = await _movieService.GetBySlugAsync(slug, cancellationToken);
            if (movie == null)
                return NotFound(ApiResponseDto<MovieDetailResponseDto>.ErrorResult($"Không tìm thấy phim với slug {slug}"));

            return Ok(ApiResponseDto<MovieDetailResponseDto>.SuccessResult(movie));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie by slug {Slug}", slug);
            return StatusCode(500, ApiResponseDto<MovieDetailResponseDto>.ErrorResult("Lỗi khi lấy thông tin phim"));
        }
    }

    /// <summary>
    /// Lấy danh sách phim đang chiếu
    /// </summary>
    [HttpGet("now-showing")]
    [ProducesResponseType(typeof(List<MovieResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieResponseDto>>> GetNowShowing(CancellationToken cancellationToken = default)
    {
        try
        {
            var movies = await _movieService.GetNowShowingAsync(cancellationToken);
            return Ok(ApiResponseDto<List<MovieResponseDto>>.SuccessResult(movies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting now showing movies");
            return StatusCode(500, ApiResponseDto<List<MovieResponseDto>>.ErrorResult("Lỗi khi lấy danh sách phim đang chiếu"));
        }
    }

    /// <summary>
    /// Lấy danh sách phim sắp chiếu
    /// </summary>
    [HttpGet("coming-soon")]
    [ProducesResponseType(typeof(List<MovieResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieResponseDto>>> GetComingSoon(CancellationToken cancellationToken = default)
    {
        try
        {
            var movies = await _movieService.GetComingSoonAsync(cancellationToken);
            return Ok(ApiResponseDto<List<MovieResponseDto>>.SuccessResult(movies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coming soon movies");
            return StatusCode(500, ApiResponseDto<List<MovieResponseDto>>.ErrorResult("Lỗi khi lấy danh sách phim sắp chiếu"));
        }
    }

    /// <summary>
    /// Tìm kiếm phim
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<MovieResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieResponseDto>>> Search([FromQuery] string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(ApiResponseDto<List<MovieResponseDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));

            var movies = await _movieService.SearchAsync(searchTerm, cancellationToken);
            return Ok(ApiResponseDto<List<MovieResponseDto>>.SuccessResult(movies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies");
            return StatusCode(500, ApiResponseDto<List<MovieResponseDto>>.ErrorResult("Lỗi khi tìm kiếm phim"));
        }
    }

    /// <summary>
    /// Tạo phim mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieResponseDto>> Create([FromBody] MovieCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<MovieResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var movie = await _movieService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, ApiResponseDto<MovieResponseDto>.SuccessResult(movie, "Tạo phim thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<MovieResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie");
            return StatusCode(500, ApiResponseDto<MovieResponseDto>.ErrorResult("Lỗi khi tạo phim"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin phim
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieResponseDto>> Update(Guid id, [FromBody] MovieUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<MovieResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var movie = await _movieService.UpdateAsync(id, dto, cancellationToken);
            if (movie == null)
                return NotFound(ApiResponseDto<MovieResponseDto>.ErrorResult($"Không tìm thấy phim có ID {id}"));

            return Ok(ApiResponseDto<MovieResponseDto>.SuccessResult(movie, "Cập nhật phim thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<MovieResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating movie {MovieId}", id);
            return StatusCode(500, ApiResponseDto<MovieResponseDto>.ErrorResult("Lỗi khi cập nhật phim"));
        }
    }

    /// <summary>
    /// Xóa phim
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _movieService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy phim có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie {MovieId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa phim"));
        }
    }
}

