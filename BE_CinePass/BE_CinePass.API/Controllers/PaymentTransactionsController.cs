using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.PaymentTransaction;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentTransactionsController : ControllerBase
{
    private readonly PaymentTransactionService _paymentTransactionService;
    private readonly ILogger<PaymentTransactionsController> _logger;

    public PaymentTransactionsController(PaymentTransactionService paymentTransactionService, ILogger<PaymentTransactionsController> logger)
    {
        _paymentTransactionService = paymentTransactionService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin giao dịch thanh toán theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentTransactionResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _paymentTransactionService.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
                return NotFound(ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult($"Không tìm thấy giao dịch thanh toán có ID {id}"));

            return Ok(ApiResponseDto<PaymentTransactionResponseDto>.SuccessResult(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment transaction by id {TransactionId}", id);
            return StatusCode(500, ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult("Lỗi khi lấy thông tin giao dịch thanh toán"));
        }
    }

    /// <summary>
    /// Lấy danh sách giao dịch thanh toán theo đơn hàng
    /// </summary>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(List<PaymentTransactionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentTransactionResponseDto>>> GetByOrderId(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _paymentTransactionService.GetByOrderIdAsync(orderId, cancellationToken);
            return Ok(ApiResponseDto<List<PaymentTransactionResponseDto>>.SuccessResult(transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment transactions by order {OrderId}", orderId);
            return StatusCode(500, ApiResponseDto<List<PaymentTransactionResponseDto>>.ErrorResult("Lỗi khi lấy danh sách giao dịch thanh toán"));
        }
    }

    /// <summary>
    /// Lấy thông tin giao dịch thanh toán theo mã giao dịch của nhà cung cấp
    /// </summary>
    [HttpGet("provider/{providerTransId}")]
    [ProducesResponseType(typeof(PaymentTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentTransactionResponseDto>> GetByProviderTransId(string providerTransId, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _paymentTransactionService.GetByProviderTransIdAsync(providerTransId, cancellationToken);
            if (transaction == null)
                return NotFound(ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult($"Không tìm thấy giao dịch thanh toán với mã {providerTransId}"));

            return Ok(ApiResponseDto<PaymentTransactionResponseDto>.SuccessResult(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment transaction by provider trans id {ProviderTransId}", providerTransId);
            return StatusCode(500, ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult("Lỗi khi lấy thông tin giao dịch thanh toán"));
        }
    }

    /// <summary>
    /// Lấy danh sách giao dịch thanh toán thành công
    /// </summary>
    [HttpGet("successful")]
    [ProducesResponseType(typeof(List<PaymentTransactionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentTransactionResponseDto>>> GetSuccessful(CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _paymentTransactionService.GetSuccessfulTransactionsAsync(cancellationToken);
            return Ok(ApiResponseDto<List<PaymentTransactionResponseDto>>.SuccessResult(transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting successful transactions");
            return StatusCode(500, ApiResponseDto<List<PaymentTransactionResponseDto>>.ErrorResult("Lỗi khi lấy danh sách giao dịch thanh toán"));
        }
    }

    /// <summary>
    /// Lấy danh sách giao dịch thanh toán thất bại
    /// </summary>
    [HttpGet("failed")]
    [ProducesResponseType(typeof(List<PaymentTransactionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentTransactionResponseDto>>> GetFailed(CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _paymentTransactionService.GetFailedTransactionsAsync(cancellationToken);
            return Ok(ApiResponseDto<List<PaymentTransactionResponseDto>>.SuccessResult(transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed transactions");
            return StatusCode(500, ApiResponseDto<List<PaymentTransactionResponseDto>>.ErrorResult("Lỗi khi lấy danh sách giao dịch thanh toán"));
        }
    }

    /// <summary>
    /// Tạo giao dịch thanh toán mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentTransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentTransactionResponseDto>> Create([FromBody] PaymentTransactionCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var transaction = await _paymentTransactionService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, ApiResponseDto<PaymentTransactionResponseDto>.SuccessResult(transaction, "Tạo giao dịch thanh toán thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment transaction");
            return StatusCode(500, ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult("Lỗi khi tạo giao dịch thanh toán"));
        }
    }

    /// <summary>
    /// Cập nhật trạng thái giao dịch thanh toán
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(PaymentTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentTransactionResponseDto>> UpdateStatus(Guid id, [FromBody] string status, CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _paymentTransactionService.UpdateStatusAsync(id, status, cancellationToken);
            if (transaction == null)
                return NotFound(ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult($"Không tìm thấy giao dịch thanh toán có ID {id}"));

            return Ok(ApiResponseDto<PaymentTransactionResponseDto>.SuccessResult(transaction, "Cập nhật trạng thái thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment transaction status {TransactionId}", id);
            return StatusCode(500, ApiResponseDto<PaymentTransactionResponseDto>.ErrorResult("Lỗi khi cập nhật trạng thái giao dịch thanh toán"));
        }
    }
}

