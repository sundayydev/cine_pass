using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Voucher;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VouchersController : ControllerBase
{
    private readonly VoucherService _voucherService;
    private readonly ILogger<VouchersController> _logger;

    public VouchersController(VoucherService voucherService, ILogger<VouchersController> logger)
    {
        _voucherService = voucherService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả voucher đang hoạt động
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VoucherResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VoucherResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var vouchers = await _voucherService.GetAllVouchersAsync(cancellationToken);
            return Ok(ApiResponseDto<List<VoucherResponseDto>>.SuccessResult(vouchers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all vouchers");
            return StatusCode(500, ApiResponseDto<List<VoucherResponseDto>>.ErrorResult("Lỗi khi lấy danh sách voucher"));
        }
    }

    /// <summary>
    /// Lấy danh sách voucher có thể đổi cho user
    /// </summary>
    [HttpGet("available/{userId}")]
    [ProducesResponseType(typeof(List<VoucherResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VoucherResponseDto>>> GetAvailableForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var vouchers = await _voucherService.GetAvailableVouchersForUserAsync(userId, cancellationToken);
            return Ok(ApiResponseDto<List<VoucherResponseDto>>.SuccessResult(vouchers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available vouchers for user {UserId}", userId);
            return StatusCode(500, ApiResponseDto<List<VoucherResponseDto>>.ErrorResult("Lỗi khi lấy danh sách voucher"));
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết voucher
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VoucherResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VoucherResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var voucher = await _voucherService.GetByIdAsync(id, cancellationToken);
            if (voucher == null)
                return NotFound(ApiResponseDto<VoucherResponseDto>.ErrorResult("Không tìm thấy voucher"));

            return Ok(ApiResponseDto<VoucherResponseDto>.SuccessResult(voucher));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher {VoucherId}", id);
            return StatusCode(500, ApiResponseDto<VoucherResponseDto>.ErrorResult("Lỗi khi lấy thông tin voucher"));
        }
    }

    /// <summary>
    /// Tạo voucher mới (Admin only)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VoucherResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VoucherResponseDto>> Create([FromBody] VoucherCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<VoucherResponseDto>.ErrorResult("Dữ liệu không hợp lệ", 
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var voucher = await _voucherService.CreateVoucherAsync(dto, cancellationToken);
            var voucherDto = await _voucherService.GetByIdAsync(voucher.Id, cancellationToken);
            
            return CreatedAtAction(nameof(GetById), new { id = voucher.Id }, 
                ApiResponseDto<VoucherResponseDto>.SuccessResult(voucherDto!, "Tạo voucher thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<VoucherResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voucher");
            return StatusCode(500, ApiResponseDto<VoucherResponseDto>.ErrorResult("Lỗi khi tạo voucher"));
        }
    }

    /// <summary>
    /// Cập nhật voucher (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VoucherResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VoucherResponseDto>> Update(Guid id, [FromBody] VoucherUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<VoucherResponseDto>.ErrorResult("Dữ liệu không hợp lệ", 
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _voucherService.UpdateVoucherAsync(id, dto, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<VoucherResponseDto>.ErrorResult("Không tìm thấy voucher"));

            var voucher = await _voucherService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponseDto<VoucherResponseDto>.SuccessResult(voucher!, "Cập nhật voucher thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voucher {VoucherId}", id);
            return StatusCode(500, ApiResponseDto<VoucherResponseDto>.ErrorResult("Lỗi khi cập nhật voucher"));
        }
    }

    /// <summary>
    /// Xóa voucher (soft delete - Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _voucherService.DeleteVoucherAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy voucher"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voucher {VoucherId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa voucher"));
        }
    }
}
