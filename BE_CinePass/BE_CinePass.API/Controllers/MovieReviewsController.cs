using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.MovieReview;
using BE_CinePass.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieReviewsController : ControllerBase
{
    private readonly MovieReviewService _reviewService;
    private readonly ILogger<MovieReviewsController> _logger;

    public MovieReviewsController(MovieReviewService reviewService, ILogger<MovieReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách đánh giá theo Movie Id
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(List<MovieReviewResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovieReviewResponseDto>>> GetByMovieId(Guid movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reviews = await _reviewService.GetByMovieIdAsync(movieId, cancellationToken);
            return Ok(ApiResponseDto<List<MovieReviewResponseDto>>.SuccessResult(reviews));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for movie {MovieId}", movieId);
            return StatusCode(500, ApiResponseDto<List<MovieReviewResponseDto>>.ErrorResult("Lỗi khi lấy danh sách đánh giá"));
        }
    }

    /// <summary>
    /// Tạo đánh giá mới (yêu cầu đăng nhập)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(MovieReviewResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MovieReviewResponseDto>> Create([FromBody] MovieReviewCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<MovieReviewResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<MovieReviewResponseDto>.ErrorResult("Không xác định được người dùng"));

            var review = await _reviewService.CreateAsync(userId.Value, dto, cancellationToken);
            return CreatedAtAction(nameof(GetByMovieId), new { movieId = dto.MovieId }, ApiResponseDto<MovieReviewResponseDto>.SuccessResult(review, "Đánh giá thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<MovieReviewResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return StatusCode(500, ApiResponseDto<MovieReviewResponseDto>.ErrorResult("Lỗi khi tạo đánh giá"));
        }
    }
}
