using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Events;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.UserVoucher;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class UserVoucherService
{
    private readonly ApplicationDbContext _context;
    private readonly VoucherService _voucherService;
    private readonly MemberPointService _memberPointService;
    private readonly PointHistoryService _pointHistoryService;
    private readonly IEventBus _eventBus;
    
    public UserVoucherService(
        ApplicationDbContext context,
        VoucherService voucherService,
        MemberPointService memberPointService,
        PointHistoryService pointHistoryService,
        IEventBus eventBus)
    {
        _context = context;
        _voucherService = voucherService;
        _memberPointService = memberPointService;
        _pointHistoryService = pointHistoryService;
        _eventBus = eventBus; 
    }

    public async Task<List<UserVoucherResponseDto>> GetUserVouchersAsync(
        Guid userId, 
        bool onlyAvailable = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserVouchers
            .Include(uv => uv.Voucher)
            .Where(uv => uv.UserId == userId);

        if (onlyAvailable)
        {
            query = query.Where(uv => !uv.IsUsed 
                                   && (uv.ExpiresAt == null || uv.ExpiresAt > DateTime.UtcNow)
                                   && uv.Voucher.Status == VoucherStatus.Active);
        }

        var userVouchers = await query
            .OrderByDescending(uv => uv.CreatedAt)
            .ToListAsync(cancellationToken);

        return userVouchers.Select(MapToResponseDto).ToList();
    }

    public async Task<UserVoucherResponseDto?> GetByIdAsync(
        Guid userVoucherId, 
        CancellationToken cancellationToken = default)
    {
        var userVoucher = await _context.UserVouchers
            .Include(uv => uv.Voucher)
            .FirstOrDefaultAsync(uv => uv.Id == userVoucherId, cancellationToken);

        return userVoucher == null ? null : MapToResponseDto(userVoucher);
    }

    public async Task<(bool Success, UserVoucherResponseDto? UserVoucher, string? ErrorMessage)> RedeemVoucherAsync(
        Guid userId,
        Guid voucherId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use execution strategy for retry compatibility
            var strategy = _context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async (ct) =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    // Validate
                    var (isValid, errorMessage) = await _voucherService.ValidateVoucherRedemptionAsync(
                        userId, voucherId, ct);

                    if (!isValid)
                    {
                        await transaction.RollbackAsync(ct);
                        return (false, (UserVoucherResponseDto?)null, errorMessage);
                    }

                    var voucher = await _context.Vouchers.FindAsync(new object[] { voucherId }, ct);
                    if (voucher == null)
                    {
                        await transaction.RollbackAsync(ct);
                        return (false, (UserVoucherResponseDto?)null, "Voucher không tồn tại");
                    }

                    // Deduct points (only if pointsRequired > 0)
                    if (voucher.PointsRequired > 0)
                    {
                        try
                        {
                            var pointsDeducted = await _memberPointService.SubtractPointsAsync(userId, voucher.PointsRequired, ct);
                            if (pointsDeducted == null)
                            {
                                await transaction.RollbackAsync(ct);
                                return (false, (UserVoucherResponseDto?)null, "Không thể trừ điểm");
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            await transaction.RollbackAsync(ct);
                            return (false, (UserVoucherResponseDto?)null, ex.Message);
                        }
                    }

                    // Create user voucher
                    var userVoucher = new UserVoucher
                    {
                        UserId = userId,
                        VoucherId = voucherId,
                        RedeemedAt = DateTime.UtcNow,
                        ExpiresAt = voucher.ValidTo ?? DateTime.UtcNow.AddMonths(3)
                    };

                    _context.UserVouchers.Add(userVoucher);

                    // Update voucher quantity
                    voucher.QuantityRedeemed++;
                    voucher.UpdatedAt = DateTime.UtcNow;

                    // Create point history (only if points were deducted)
                    if (voucher.PointsRequired > 0)
                    {
                        await _pointHistoryService.CreateAsync(new Domain.Models.PointHistory
                        {
                            UserId = userId,
                            Points = -voucher.PointsRequired,
                            Type = PointHistoryType.RedeemVoucher,
                            Description = $"Đổi voucher {voucher.Name}",
                            VoucherId = voucherId
                        }, ct);
                    }

                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    // Reload to include voucher info
                    await _context.Entry(userVoucher).Reference(uv => uv.Voucher).LoadAsync(ct);
                    await _eventBus.PublishAsync(new VoucherReceivedEvent
                    {
                        UserId = userId,
                        VoucherId = userVoucher.Id,
                        VoucherCode = voucher.Code,
                        VoucherName = voucher.Name,
                        DiscountValue = voucher.DiscountValue,
                        VoucherType = voucher.Type.ToString(),
                        ExpiresAt = userVoucher.ExpiresAt ?? DateTime.UtcNow.AddMonths(3)
                    });
                    return (true, MapToResponseDto(userVoucher), (string?)null);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, null, $"Lỗi khi đổi voucher: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ValidateVoucherUsageAsync(
        Guid userVoucherId,
        decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        var userVoucher = await _context.UserVouchers
            .Include(uv => uv.Voucher)
            .FirstOrDefaultAsync(uv => uv.Id == userVoucherId, cancellationToken);

        if (userVoucher == null)
            return (false, "Voucher không tồn tại");

        if (userVoucher.IsUsed)
            return (false, "Voucher đã được sử dụng");

        if (userVoucher.ExpiresAt.HasValue && userVoucher.ExpiresAt.Value < DateTime.UtcNow)
            return (false, "Voucher đã hết hạn");

        if (userVoucher.Voucher.Status != VoucherStatus.Active)
            return (false, "Voucher không còn hiệu lực");

        if (userVoucher.Voucher.MinOrderAmount.HasValue && orderAmount < userVoucher.Voucher.MinOrderAmount.Value)
            return (false, $"Đơn hàng tối thiểu {userVoucher.Voucher.MinOrderAmount:N0}đ");

        return (true, null);
    }

    public decimal CalculateDiscount(UserVoucher userVoucher, decimal orderAmount)
    {
        if (userVoucher.Voucher.Type == VoucherType.FixedAmount)
        {
            return Math.Min(userVoucher.Voucher.DiscountValue, orderAmount);
        }
        else // Percentage
        {
            var discount = orderAmount * (userVoucher.Voucher.DiscountValue / 100);
            
            if (userVoucher.Voucher.MaxDiscountAmount.HasValue)
            {
                discount = Math.Min(discount, userVoucher.Voucher.MaxDiscountAmount.Value);
            }
            
            return Math.Min(discount, orderAmount);
        }
    }

    public async Task<bool> MarkAsUsedAsync(
        Guid userVoucherId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var userVoucher = await _context.UserVouchers.FindAsync(new object[] { userVoucherId }, cancellationToken);
        if (userVoucher == null) return false;

        userVoucher.IsUsed = true;
        userVoucher.UsedAt = DateTime.UtcNow;
        userVoucher.OrderId = orderId;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserVoucherResponseDto MapToResponseDto(UserVoucher userVoucher)
    {
        var now = DateTime.UtcNow;
        var isExpired = userVoucher.ExpiresAt.HasValue && userVoucher.ExpiresAt.Value < now;
        var canUse = !userVoucher.IsUsed 
                    && !isExpired 
                    && userVoucher.Voucher.Status == VoucherStatus.Active;

        string? reasonCannotUse = null;
        if (userVoucher.IsUsed)
            reasonCannotUse = "Đã sử dụng";
        else if (isExpired)
            reasonCannotUse = "Đã hết hạn";
        else if (userVoucher.Voucher.Status != VoucherStatus.Active)
            reasonCannotUse = "Voucher không còn hiệu lực";

        return new UserVoucherResponseDto
        {
            Id = userVoucher.Id,
            UserId = userVoucher.UserId,
            VoucherId = userVoucher.VoucherId,
            IsUsed = userVoucher.IsUsed,
            UsedAt = userVoucher.UsedAt,
            OrderId = userVoucher.OrderId,
            RedeemedAt = userVoucher.RedeemedAt,
            ExpiresAt = userVoucher.ExpiresAt,
            CreatedAt = userVoucher.CreatedAt,
            VoucherCode = userVoucher.Voucher.Code,
            VoucherName = userVoucher.Voucher.Name,
            VoucherDescription = userVoucher.Voucher.Description,
            VoucherImageUrl = userVoucher.Voucher.ImageUrl,
            VoucherType = userVoucher.Voucher.Type.ToString(),
            DiscountValue = userVoucher.Voucher.DiscountValue,
            MaxDiscountAmount = userVoucher.Voucher.MaxDiscountAmount,
            MinOrderAmount = userVoucher.Voucher.MinOrderAmount,
            VoucherValidTo = userVoucher.Voucher.ValidTo,
            IsExpired = isExpired,
            CanUse = canUse,
            ReasonCannotUse = reasonCannotUse
        };
    }
}
