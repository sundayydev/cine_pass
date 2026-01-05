using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.PaymentTransaction;
using System.Text.Json;
using BE_CinePass.Domain.Events;

namespace BE_CinePass.Core.Services;

public class PaymentTransactionService
{
    private readonly PaymentTransactionRepository _paymentTransactionRepository;
    private readonly OrderRepository _orderRepository;
    private readonly ApplicationDbContext _context;
    private readonly IEventBus _eventBus;

    public PaymentTransactionService(PaymentTransactionRepository paymentTransactionRepository, OrderRepository orderRepository, ApplicationDbContext context, IEventBus eventBus)
    {
        _paymentTransactionRepository = paymentTransactionRepository;
        _orderRepository = orderRepository;
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<PaymentTransactionResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentTransactionRepository.GetByIdAsync(id, cancellationToken);
        return transaction == null ? null : MapToResponseDto(transaction);
    }

    public async Task<List<PaymentTransactionResponseDto>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var transactions = await _paymentTransactionRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return transactions.Select(MapToResponseDto).ToList();
    }

    public async Task<PaymentTransactionResponseDto?> GetByProviderTransIdAsync(string providerTransId, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentTransactionRepository.GetByProviderTransIdAsync(providerTransId, cancellationToken);
        return transaction == null ? null : MapToResponseDto(transaction);
    }

    public async Task<PaymentTransactionResponseDto> CreateAsync(PaymentTransactionCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validate order exists
        var order = await _orderRepository.GetByIdAsync(dto.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Không tìm thấy đơn hàng có ID {dto.OrderId}");

        JsonDocument? responseJson = null;
        if (!string.IsNullOrEmpty(dto.ResponseJson))
        {
            try
            {
                responseJson = JsonDocument.Parse(dto.ResponseJson);
            }
            catch
            {
                throw new InvalidOperationException("Định dạng JSON không hợp lệ cho ResponseJson");
            }
        }

        var transaction = new PaymentTransaction
        {
            OrderId = dto.OrderId,
            ProviderTransId = dto.ProviderTransId,
            Amount = dto.Amount,
            Status = dto.Status,
            ResponseJson = responseJson,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        // Khi payment thành công
        if (transaction.Status == "Success" || transaction.Status == "Completed")
        {
            var orderCode = $"ORD-{order.Id.ToString()[..8].ToUpper()}";
            await _eventBus.PublishAsync(new PaymentSuccessEvent
            {
                OrderId = transaction.OrderId.Value,
                UserId = order.UserId,
                OrderCode = orderCode,
                Amount = transaction.Amount.Value,
                PaymentMethod = "MOMO"
            });
        }
        // Khi payment thất bại
        else if (transaction.Status == "Failed" || transaction.Status == "Cancelled")
        {
            var orderCode = $"ORD-{order.Id.ToString()[..8].ToUpper()}";
            await _eventBus.PublishAsync(new PaymentFailedEvent
            {
                OrderId = transaction.OrderId.Value,
                UserId = order.UserId,
                OrderCode = orderCode,
                Reason = "Payment failed"
            });
        }
        return MapToResponseDto(transaction);
    }

    public async Task<PaymentTransactionResponseDto?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentTransactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return null;

        transaction.Status = status;

        _paymentTransactionRepository.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(transaction);
    }

    public async Task<List<PaymentTransactionResponseDto>> GetSuccessfulTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await _paymentTransactionRepository.GetSuccessfulTransactionsAsync(cancellationToken);
        return transactions.Select(MapToResponseDto).ToList();
    }

    public async Task<List<PaymentTransactionResponseDto>> GetFailedTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await _paymentTransactionRepository.GetFailedTransactionsAsync(cancellationToken);
        return transactions.Select(MapToResponseDto).ToList();
    }

    public async Task<PaymentTransactionResponseDto?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentTransactionRepository.GetByProviderTransIdAsync(transactionId, cancellationToken);
        return transaction == null ? null : MapToResponseDto(transaction);
    }

    public async Task<PaymentTransactionResponseDto?> UpdateTransactionIdAsync(Guid id, string transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentTransactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return null;

        transaction.ProviderTransId = transactionId;

        _paymentTransactionRepository.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(transaction);
    }

    public async Task<PaymentTransactionResponseDto?> UpdateTransactionWithResponseAsync(
        Guid id,
        string transactionId,
        string responseJsonString,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentTransactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null)
            return null;

        transaction.ProviderTransId = transactionId;

        try
        {
            transaction.ResponseJson = JsonDocument.Parse(responseJsonString);
        }
        catch
        {
            // If JSON parsing fails, keep existing ResponseJson
        }

        _paymentTransactionRepository.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(transaction);
    }

    public async Task<string?> GetMomoOrderIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction?.ResponseJson == null)
            return null;

        try
        {
            var root = transaction.ResponseJson.RootElement;
            if (root.TryGetProperty("momoOrderId", out var momoOrderIdElement))
            {
                return momoOrderIdElement.GetString();
            }
        }
        catch
        {
            // JSON parsing error
        }

        return null;
    }


    private static PaymentTransactionResponseDto MapToResponseDto(PaymentTransaction transaction)
    {
        return new PaymentTransactionResponseDto
        {
            Id = transaction.Id,
            OrderId = transaction.OrderId,
            ProviderTransId = transaction.ProviderTransId,
            Amount = transaction.Amount,
            Status = transaction.Status,
            ResponseJson = transaction.ResponseJson?.RootElement.GetRawText(),
            CreatedAt = transaction.CreatedAt
        };
    }
}

