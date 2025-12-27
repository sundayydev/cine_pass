using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.MemberPoint;
using BE_CinePass.Shared.DTOs.MemberTierConfig;
using BE_CinePass.Shared.DTOs.PointHistory;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipController : ControllerBase
{
    private readonly MemberPointService _memberPointService;
    private readonly MemberTierConfigService _tierConfigService;
    private readonly PointHistoryService _pointHistoryService;
    private readonly ILogger<MembershipController> _logger;

    public MembershipController(
        MemberPointService memberPointService,
        MemberTierConfigService tierConfigService,
        PointHistoryService pointHistoryService,
        ILogger<MembershipController> logger)
    {
        _memberPointService = memberPointService;
        _tierConfigService = tierConfigService;
        _pointHistoryService = pointHistoryService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả cấp bậc hội viên
    /// </summary>
    [HttpGet("tiers")]
    [ProducesResponseType(typeof(List<MemberTierConfigResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MemberTierConfigResponseDto>>> GetAllTiers(CancellationToken cancellationToken = default)
    {
        try
        {
            var tiers = await _tierConfigService.GetAllTiersAsync(cancellationToken);
            return Ok(ApiResponseDto<List<MemberTierConfigResponseDto>>.SuccessResult(tiers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member tiers");
            return StatusCode(500, ApiResponseDto<List<MemberTierConfigResponseDto>>.ErrorResult("Lỗi khi lấy danh sách cấp bậc"));
        }
    }

    /// <summary>
    /// Tạo MemberPoint cho user mới
    /// </summary>
    [HttpPost("member-points")]
    [ProducesResponseType(typeof(MemberPointResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberPointResponseDto>> CreateMemberPoint(
        [FromBody] MemberPointCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<MemberPointResponseDto>.ErrorResult("Dữ liệu không hợp lệ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var memberPoint = await _memberPointService.CreateAsync(dto.UserId, dto.InitialPoints, cancellationToken);
            
            return CreatedAtAction(nameof(GetMemberProfile), new { userId = dto.UserId },
                ApiResponseDto<MemberPointResponseDto>.SuccessResult(memberPoint, "Tạo MemberPoint thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<MemberPointResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member point for user {UserId}", dto.UserId);
            return StatusCode(500, ApiResponseDto<MemberPointResponseDto>.ErrorResult("Lỗi khi tạo MemberPoint"));
        }
    }

    /// <summary>
    /// Tạo cấu hình tier mới (Admin only)
    /// </summary>
    [HttpPost("tiers")]
    [ProducesResponseType(typeof(MemberTierConfigResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberTierConfigResponseDto>> CreateTierConfig(
        [FromBody] MemberTierConfigCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<MemberTierConfigResponseDto>.ErrorResult("Dữ liệu không hợp lệ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var config = await _tierConfigService.CreateTierConfigAsync(dto, cancellationToken);
            var tierDto = await _tierConfigService.GetByTierAsync(config.Tier, cancellationToken);

            return CreatedAtAction(nameof(GetAllTiers), null,
                ApiResponseDto<MemberTierConfigResponseDto>.SuccessResult(tierDto!, "Tạo cấu hình tier thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<MemberTierConfigResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tier config");
            return StatusCode(500, ApiResponseDto<MemberTierConfigResponseDto>.ErrorResult("Lỗi khi tạo cấu hình tier"));
        }
    }

    /// <summary>
    /// Cập nhật cấu hình tier (Admin only)
    /// </summary>
    [HttpPut("tiers/{id}")]
    [ProducesResponseType(typeof(MemberTierConfigResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberTierConfigResponseDto>> UpdateTierConfig(
        Guid id,
        [FromBody] MemberTierConfigUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<MemberTierConfigResponseDto>.ErrorResult("Dữ liệu không hợp lệ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _tierConfigService.UpdateTierConfigAsync(id, dto, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<MemberTierConfigResponseDto>.ErrorResult("Không tìm thấy cấu hình tier"));

            var tiers = await _tierConfigService.GetAllTiersAsync(cancellationToken);
            var updatedTier = tiers.FirstOrDefault(t => t.Id == id);

            return Ok(ApiResponseDto<MemberTierConfigResponseDto>.SuccessResult(updatedTier!, "Cập nhật tier thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tier config {TierId}", id);
            return StatusCode(500, ApiResponseDto<MemberTierConfigResponseDto>.ErrorResult("Lỗi khi cập nhật tier"));
        }
    }

    /// <summary>
    /// Xóa cấu hình tier (Admin only)
    /// </summary>
    [HttpDelete("tiers/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTierConfig(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _tierConfigService.DeleteTierConfigAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy cấu hình tier"));

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tier config {TierId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa tier"));
        }
    }

    /// <summary>
    /// Lấy thông tin hội viên theo User ID
    /// </summary>
    [HttpGet("profile/{userId}")]
    [ProducesResponseType(typeof(MemberPointResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberPointResponseDto>> GetMemberProfile(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var memberPoint = await _memberPointService.GetByUserIdAsync(userId, cancellationToken);
            if (memberPoint == null)
                return NotFound(ApiResponseDto<MemberPointResponseDto>.ErrorResult("Không tìm thấy thông tin hội viên"));

            return Ok(ApiResponseDto<MemberPointResponseDto>.SuccessResult(memberPoint));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member profile for user {UserId}", userId);
            return StatusCode(500, ApiResponseDto<MemberPointResponseDto>.ErrorResult("Lỗi khi lấy thông tin hội viên"));
        }
    }

    /// <summary>
    /// Lấy lịch sử điểm của user
    /// </summary>
    [HttpGet("point-history/{userId}")]
    [ProducesResponseType(typeof(List<PointHistoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PointHistoryResponseDto>>> GetPointHistory(
        Guid userId,
        [FromQuery] int? limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _pointHistoryService.GetByUserIdAsync(userId, limit, cancellationToken);
            return Ok(ApiResponseDto<List<PointHistoryResponseDto>>.SuccessResult(history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting point history for user {UserId}", userId);
            return StatusCode(500, ApiResponseDto<List<PointHistoryResponseDto>>.ErrorResult("Lỗi khi lấy lịch sử điểm"));
        }
    }
}
