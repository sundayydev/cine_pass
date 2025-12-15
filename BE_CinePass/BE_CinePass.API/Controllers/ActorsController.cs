using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Actor;
using BE_CinePass.Shared.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActorsController : ControllerBase
{
    private readonly ActorService _actorService;
    private readonly ILogger<ActorsController> _logger;

    public ActorsController(ActorService actorService, ILogger<ActorsController> logger)
    {
        _actorService = actorService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả diễn viên
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ActorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ActorResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var actors = await _actorService.GetAllAsync(cancellationToken);
            return Ok(ApiResponseDto<List<ActorResponseDto>>.SuccessResult(actors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all actors");
            return StatusCode(500, ApiResponseDto<List<ActorResponseDto>>.ErrorResult("Lỗi khi lấy danh sách diễn viên"));
        }
    }

    /// <summary>
    /// Lấy thông tin diễn viên theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ActorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActorResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = await _actorService.GetByIdAsync(id, cancellationToken);
            if (actor == null)
                return NotFound(ApiResponseDto<ActorResponseDto>.ErrorResult($"Không tìm thấy diễn viên có ID {id}"));

            return Ok(ApiResponseDto<ActorResponseDto>.SuccessResult(actor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actor by id {ActorId}", id);
            return StatusCode(500, ApiResponseDto<ActorResponseDto>.ErrorResult("Lỗi khi lấy thông tin diễn viên"));
        }
    }

    /// <summary>
    /// Lấy thông tin diễn viên theo slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ActorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActorResponseDto>> GetBySlug(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = await _actorService.GetBySlugAsync(slug, cancellationToken);
            if (actor == null)
                return NotFound(ApiResponseDto<ActorResponseDto>.ErrorResult($"Không tìm thấy diễn viên với slug {slug}"));

            return Ok(ApiResponseDto<ActorResponseDto>.SuccessResult(actor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actor by slug {Slug}", slug);
            return StatusCode(500, ApiResponseDto<ActorResponseDto>.ErrorResult("Lỗi khi lấy thông tin diễn viên"));
        }
    }

    /// <summary>
    /// Tìm kiếm diễn viên
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ActorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ActorResponseDto>>> Search([FromQuery] string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(ApiResponseDto<List<ActorResponseDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));

            var actors = await _actorService.SearchAsync(searchTerm, cancellationToken);
            return Ok(ApiResponseDto<List<ActorResponseDto>>.SuccessResult(actors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching actors");
            return StatusCode(500, ApiResponseDto<List<ActorResponseDto>>.ErrorResult("Lỗi khi tìm kiếm diễn viên"));
        }
    }

    /// <summary>
    /// Tạo diễn viên mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ActorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActorResponseDto>> Create([FromBody] ActorCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ActorResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var actor = await _actorService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = actor.Id }, ApiResponseDto<ActorResponseDto>.SuccessResult(actor, "Tạo diễn viên thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ActorResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating actor");
            return StatusCode(500, ApiResponseDto<ActorResponseDto>.ErrorResult("Lỗi khi tạo diễn viên"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin diễn viên
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ActorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActorResponseDto>> Update(Guid id, [FromBody] ActorUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<ActorResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var actor = await _actorService.UpdateAsync(id, dto, cancellationToken);
            if (actor == null)
                return NotFound(ApiResponseDto<ActorResponseDto>.ErrorResult($"Không tìm thấy diễn viên có ID {id}"));

            return Ok(ApiResponseDto<ActorResponseDto>.SuccessResult(actor, "Cập nhật diễn viên thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ActorResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating actor {ActorId}", id);
            return StatusCode(500, ApiResponseDto<ActorResponseDto>.ErrorResult("Lỗi khi cập nhật diễn viên"));
        }
    }

    /// <summary>
    /// Xóa diễn viên
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _actorService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy diễn viên có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting actor {ActorId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa diễn viên"));
        }
    }
}
