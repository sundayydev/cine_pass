using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.ETicket;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ETicketsController : ControllerBase
{
    private readonly ETicketService _eTicketService;
    private readonly ILogger<ETicketsController> _logger;

    public ETicketsController(ETicketService eTicketService, ILogger<ETicketsController> logger)
    {
        _eTicketService = eTicketService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin vé điện tử theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ETicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ETicketResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var eTicket = await _eTicketService.GetByIdAsync(id, cancellationToken);
            if (eTicket == null)
                return NotFound(ApiResponseDto<ETicketResponseDto>.ErrorResult($"Không tìm thấy vé điện tử có ID {id}"));

            return Ok(ApiResponseDto<ETicketResponseDto>.SuccessResult(eTicket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting e-ticket by id {ETicketId}", id);
            return StatusCode(500, ApiResponseDto<ETicketResponseDto>.ErrorResult("Lỗi khi lấy thông tin vé điện tử"));
        }
    }

    /// <summary>
    /// Lấy thông tin vé điện tử theo mã vé
    /// </summary>
    [HttpGet("code/{ticketCode}")]
    [ProducesResponseType(typeof(ETicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ETicketResponseDto>> GetByTicketCode(string ticketCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var eTicket = await _eTicketService.GetByTicketCodeAsync(ticketCode, cancellationToken);
            if (eTicket == null)
                return NotFound(ApiResponseDto<ETicketResponseDto>.ErrorResult($"Không tìm thấy vé điện tử với mã {ticketCode}"));

            return Ok(ApiResponseDto<ETicketResponseDto>.SuccessResult(eTicket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting e-ticket by code {TicketCode}", ticketCode);
            return StatusCode(500, ApiResponseDto<ETicketResponseDto>.ErrorResult("Lỗi khi lấy thông tin vé điện tử"));
        }
    }

    /// <summary>
    /// Lấy chi tiết vé điện tử theo mã vé
    /// </summary>
    [HttpGet("code/{ticketCode}/detail")]
    [ProducesResponseType(typeof(ETicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ETicketDetailDto>> GetDetailByTicketCode(string ticketCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var eTicket = await _eTicketService.GetDetailByTicketCodeAsync(ticketCode, cancellationToken);
            if (eTicket == null)
                return NotFound(ApiResponseDto<ETicketDetailDto>.ErrorResult($"Không tìm thấy vé điện tử với mã {ticketCode}"));

            return Ok(ApiResponseDto<ETicketDetailDto>.SuccessResult(eTicket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting e-ticket detail by code {TicketCode}", ticketCode);
            return StatusCode(500, ApiResponseDto<ETicketDetailDto>.ErrorResult("Lỗi khi lấy chi tiết vé điện tử"));
        }
    }

    /// <summary>
    /// Lấy danh sách vé điện tử theo order ticket ID
    /// </summary>
    [HttpGet("order-ticket/{orderTicketId}")]
    [ProducesResponseType(typeof(List<ETicketResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ETicketResponseDto>>> GetByOrderTicketId(Guid orderTicketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var eTickets = await _eTicketService.GetByOrderTicketIdAsync(orderTicketId, cancellationToken);
            return Ok(ApiResponseDto<List<ETicketResponseDto>>.SuccessResult(eTickets));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting e-tickets by order ticket {OrderTicketId}", orderTicketId);
            return StatusCode(500, ApiResponseDto<List<ETicketResponseDto>>.ErrorResult("Lỗi khi lấy danh sách vé điện tử"));
        }
    }

    /// <summary>
    /// Tạo vé điện tử (sau khi thanh toán thành công)
    /// </summary>
    [HttpPost("generate/{orderTicketId}")]
    [ProducesResponseType(typeof(ETicketResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ETicketResponseDto>> GenerateETicket(Guid orderTicketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var eTicket = await _eTicketService.GenerateETicketAsync(orderTicketId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = eTicket.Id }, ApiResponseDto<ETicketResponseDto>.SuccessResult(eTicket, "Tạo vé điện tử thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ETicketResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating e-ticket");
            return StatusCode(500, ApiResponseDto<ETicketResponseDto>.ErrorResult("Lỗi khi tạo vé điện tử"));
        }
    }

    /// <summary>
    /// Xác thực vé điện tử
    /// </summary>
    [HttpGet("validate/{ticketCode}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ValidateTicket(string ticketCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var isValid = await _eTicketService.ValidateTicketAsync(ticketCode, cancellationToken);
            return Ok(ApiResponseDto<bool>.SuccessResult(isValid));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ticket {TicketCode}", ticketCode);
            return StatusCode(500, ApiResponseDto<bool>.ErrorResult("Lỗi khi xác thực vé"));
        }
    }

    /// <summary>
    /// Sử dụng vé (check-in)
    /// </summary>
    [HttpPost("use/{ticketCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UseTicket(string ticketCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _eTicketService.UseTicketAsync(ticketCode, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy vé điện tử với mã {ticketCode}"));

            return Ok(ApiResponseDto<object>.SuccessResult("Check-in thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using ticket {TicketCode}", ticketCode);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi check-in vé"));
        }
    }

    /// <summary>
    /// Check-in vé bằng mã QR (quét QR code)
    /// </summary>
    /// <param name="request">Request chứa dữ liệu QR</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Kết quả check-in bao gồm trạng thái và thông tin vé</returns>
    [HttpPost("checkin")]
    [ProducesResponseType(typeof(TicketVerificationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketVerificationResultDto>> CheckinByQr([FromBody] VerifyTicketDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.QrData))
                return BadRequest(ApiResponseDto<TicketVerificationResultDto>.ErrorResult("Dữ liệu QR không được để trống"));

            var result = await _eTicketService.VerifyAndUseTicketAsync(request.QrData, cancellationToken);

            if (!result.IsValid)
            {
                // Return result with success=false but include the verification details
                return Ok(new ApiResponseDto<TicketVerificationResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }

            return Ok(ApiResponseDto<TicketVerificationResultDto>.SuccessResult(result, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in ticket with QR data");
            return StatusCode(500, ApiResponseDto<TicketVerificationResultDto>.ErrorResult("Lỗi khi check-in vé"));
        }
    }
}

