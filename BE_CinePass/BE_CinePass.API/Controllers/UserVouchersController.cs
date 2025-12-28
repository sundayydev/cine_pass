using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.UserVoucher;
using BE_CinePass.Shared.DTOs.Voucher;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserVouchersController : ControllerBase
{
    private readonly UserVoucherService _userVoucherService;
    private readonly ILogger<UserVouchersController> _logger;

    public UserVouchersController(UserVoucherService userVoucherService, ILogger<UserVouchersController> logger)
    {
        _userVoucherService = userVoucherService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách voucher của user
    /// </summary>
[HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<UserVoucherResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserVoucherResponseDto>>> GetUserVouchers(
        Guid userId,
        [FromQuery] bool onlyAvailable = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var vouchers = await _userVoucherService.GetUserVouchersAsync(userId, onlyAvailable, cancellationToken);
            return Ok(ApiResponseDto<List<UserVoucherResponseDto>>.SuccessResult(vouchers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user vouchers for {UserId}", userId);
            return StatusCode(500, ApiResponseDto<List<UserVoucherResponseDto>>.ErrorResult("Lỗi khi lấy danh sách voucher"));
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết user voucher
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserVoucherResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserVoucherResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userVoucher = await _userVoucherService.GetByIdAsync(id, cancellationToken);
            if (userVoucher == null)
                return NotFound(ApiResponseDto<UserVoucherResponseDto>.ErrorResult("Không tìm thấy voucher"));

            return Ok(ApiResponseDto<UserVoucherResponseDto>.SuccessResult(userVoucher));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user voucher {UserVoucherId}", id);
            return StatusCode(500, ApiResponseDto<UserVoucherResponseDto>.ErrorResult("Lỗi khi lấy thông tin voucher"));
        }
    }

    /// <summary>
    /// Đổi điểm lấy voucher
    /// </summary>
    [HttpPost("redeem")]
    [ProducesResponseType(typeof(UserVoucherResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserVoucherResponseDto>> RedeemVoucher(
        [FromBody] RedeemVoucherDto dto,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<UserVoucherResponseDto>.ErrorResult("Dữ liệu không hợp lệ", 
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var (success, userVoucher, errorMessage) = await _userVoucherService.RedeemVoucherAsync(
                userId, dto.VoucherId, cancellationToken);

            if (!success)
                return BadRequest(ApiResponseDto<UserVoucherResponseDto>.ErrorResult(errorMessage ?? "Không thể đổi voucher"));

            return CreatedAtAction(nameof(GetById), new { id = userVoucher!.Id }, 
                ApiResponseDto<UserVoucherResponseDto>.SuccessResult(userVoucher, "Đổi voucher thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming voucher for user {UserId}", userId);
            return StatusCode(500, ApiResponseDto<UserVoucherResponseDto>.ErrorResult("Lỗi khi đổi voucher"));
        }
    }

    /// <summary>
    /// Validate voucher trước khi sử dụng
    /// </summary>
    [HttpPost("{userVoucherId}/validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ValidateVoucherUsage(
        Guid userVoucherId,
        [FromQuery] decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (success, errorMessage) = await _userVoucherService.ValidateVoucherUsageAsync(
                userVoucherId, orderAmount, cancellationToken);

            if (!success)
                return Ok(ApiResponseDto<bool>.ErrorResult(errorMessage ?? "Voucher không hợp lệ"));

            return Ok(ApiResponseDto<bool>.SuccessResult(true, "Voucher hợp lệ"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user voucher {UserVoucherId}", userVoucherId);
            return StatusCode(500, ApiResponseDto<bool>.ErrorResult("Lỗi khi validate voucher"));
        }
    }
}
