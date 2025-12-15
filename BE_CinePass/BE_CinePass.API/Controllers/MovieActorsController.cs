using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.MovieActor;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieActorsController : ControllerBase
{
    private readonly MovieActorService _movieActorService;
    private readonly ILogger<MovieActorsController> _logger;

    public MovieActorsController(MovieActorService movieActorService, ILogger<MovieActorsController> logger)
    {
        _movieActorService = movieActorService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả các mối quan hệ phim-diễn viên
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MovieActorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieActorResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var movieActors = await _movieActorService.GetAllAsync(cancellationToken);
            return Ok(ApiResponseDto<List<MovieActorResponseDto>>.SuccessResult(movieActors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all movie actors");
            return StatusCode(500, ApiResponseDto<List<MovieActorResponseDto>>.ErrorResult("Lỗi khi lấy danh sách phim-diễn viên"));
        }
    }

    /// <summary>
    /// Lấy thông tin mối quan hệ phim-diễn viên theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MovieActorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieActorResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var movieActor = await _movieActorService.GetByIdAsync(id, cancellationToken);
            if (movieActor == null)
                return NotFound(ApiResponseDto<MovieActorResponseDto>.ErrorResult($"Không tìm thấy mối quan hệ có ID {id}"));

            return Ok(ApiResponseDto<MovieActorResponseDto>.SuccessResult(movieActor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie actor by id {Id}", id);
            return StatusCode(500, ApiResponseDto<MovieActorResponseDto>.ErrorResult("Lỗi khi lấy thông tin phim-diễn viên"));
        }
    }

    /// <summary>
    /// Lấy danh sách diễn viên của một phim
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(List<MovieActorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieActorResponseDto>>> GetByMovieId(Guid movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            var movieActors = await _movieActorService.GetByMovieIdAsync(movieId, cancellationToken);
            return Ok(ApiResponseDto<List<MovieActorResponseDto>>.SuccessResult(movieActors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actors for movie {MovieId}", movieId);
            return StatusCode(500, ApiResponseDto<List<MovieActorResponseDto>>.ErrorResult("Lỗi khi lấy danh sách diễn viên của phim"));
        }
    }

    /// <summary>
    /// Lấy danh sách phim của một diễn viên
    /// </summary>
    [HttpGet("actor/{actorId}")]
    [ProducesResponseType(typeof(List<MovieActorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieActorResponseDto>>> GetByActorId(Guid actorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var movieActors = await _movieActorService.GetByActorIdAsync(actorId, cancellationToken);
            return Ok(ApiResponseDto<List<MovieActorResponseDto>>.SuccessResult(movieActors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movies for actor {ActorId}", actorId);
            return StatusCode(500, ApiResponseDto<List<MovieActorResponseDto>>.ErrorResult("Lỗi khi lấy danh sách phim của diễn viên"));
        }
    }

    /// <summary>
    /// Thêm diễn viên vào phim
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MovieActorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieActorResponseDto>> Create([FromBody] MovieActorCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<MovieActorResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var movieActor = await _movieActorService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = movieActor.Id }, ApiResponseDto<MovieActorResponseDto>.SuccessResult(movieActor, "Thêm diễn viên vào phim thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<MovieActorResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie actor");
            return StatusCode(500, ApiResponseDto<MovieActorResponseDto>.ErrorResult("Lỗi khi thêm diễn viên vào phim"));
        }
    }

    /// <summary>
    /// Xóa diễn viên khỏi phim
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _movieActorService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy mối quan hệ có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie actor {Id}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa diễn viên khỏi phim"));
        }
    }
}
