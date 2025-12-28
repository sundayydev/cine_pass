using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.MemberPoint;
using BE_CinePass.Shared.DTOs.Voucher;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class VoucherService
{
    private readonly ApplicationDbContext _context;
    private readonly MemberPointService _memberPointService;

    public VoucherService(ApplicationDbContext context, MemberPointService memberPointService)
    {
        _context = context;
        _memberPointService = memberPointService;
    }

    public async Task<List<VoucherResponseDto>> GetAllVouchersAsync(CancellationToken cancellationToken = default)
    {
        var vouchers = await _context.Vouchers
            .Where(v => v.Status == VoucherStatus.Active)
            .OrderBy(v => v.PointsRequired)
            .ToListAsync(cancellationToken);

        return vouchers.Select(MapToResponseDto).ToList();
    }

    public async Task<List<VoucherResponseDto>> GetAvailableVouchersForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointService.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null) return new List<VoucherResponseDto>();

        var vouchers = await _context.Vouchers
            .Where(v => v.Status == VoucherStatus.Active
                     && (v.ValidFrom == null || v.ValidFrom <= DateTime.UtcNow)
                     && (v.ValidTo == null || v.ValidTo >= DateTime.UtcNow))
            .ToListAsync(cancellationToken);

        var result = new List<VoucherResponseDto>();

        foreach (var voucher in vouchers)
        {
            var dto = MapToResponseDto(voucher);
            
            // Check if user can redeem
            var (canRedeem, reason) = CanUserRedeemVoucher(voucher, memberPoint);
            dto.CanRedeem = canRedeem;
            dto.ReasonCannotRedeem = reason;

            result.Add(dto);
        }

        return result.OrderByDescending(v => v.CanRedeem)
                     .ThenBy(v => v.PointsRequired)
                     .ToList();
    }

    public async Task<VoucherResponseDto?> GetByIdAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        var voucher = await _context.Vouchers.FindAsync(new object[] { voucherId }, cancellationToken);
        return voucher == null ? null : MapToResponseDto(voucher);
    }

    public async Task<Voucher> CreateVoucherAsync(VoucherCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Check if code already exists
        if (await _context.Vouchers.AnyAsync(v => v.Code == dto.Code.ToUpper(), cancellationToken))
            throw new InvalidOperationException($"Voucher code '{dto.Code}' already exists");

        var voucher = new Voucher
        {
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Type = Enum.Parse<VoucherType>(dto.Type),
            DiscountValue = dto.DiscountValue,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MinOrderAmount = dto.MinOrderAmount,
            PointsRequired = dto.PointsRequired,
            Quantity = dto.Quantity,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            MinTier = string.IsNullOrEmpty(dto.MinTier) ? null : Enum.Parse<MemberTier>(dto.MinTier),
            Status = VoucherStatus.Active
        };

        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync(cancellationToken);

        return voucher;
    }

    public async Task<bool> UpdateVoucherAsync(Guid voucherId, VoucherUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var voucher = await _context.Vouchers.FindAsync(new object[] { voucherId }, cancellationToken);
        if (voucher == null) return false;

        if (dto.Name != null) voucher.Name = dto.Name;
        if (dto.Description != null) voucher.Description = dto.Description;
        if (dto.ImageUrl != null) voucher.ImageUrl = dto.ImageUrl;
        if (dto.Status != null) voucher.Status = Enum.Parse<VoucherStatus>(dto.Status);
        if (dto.Quantity.HasValue) voucher.Quantity = dto.Quantity;
        if (dto.ValidFrom.HasValue) voucher.ValidFrom = dto.ValidFrom;
        if (dto.ValidTo.HasValue) voucher.ValidTo = dto.ValidTo;

        voucher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        var voucher = await _context.Vouchers.FindAsync(new object[] { voucherId }, cancellationToken);
        if (voucher == null) return false;

        voucher.Status = VoucherStatus.Inactive;
        voucher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(bool Success, string? ErrorMessage)> ValidateVoucherRedemptionAsync(
        Guid userId, 
        Guid voucherId, 
        CancellationToken cancellationToken = default)
    {
        var voucher = await _context.Vouchers.FindAsync(new object[] { voucherId }, cancellationToken);
        if (voucher == null) return (false, "Voucher không tồn tại");

        if (voucher.Status != VoucherStatus.Active) 
            return (false, "Voucher không còn hiệu lực");

        if (voucher.ValidFrom.HasValue && DateTime.UtcNow < voucher.ValidFrom.Value)
            return (false, "Voucher chưa đến ngày áp dụng");

        if (voucher.ValidTo.HasValue && DateTime.UtcNow > voucher.ValidTo.Value)
            return (false, "Voucher đã hết hạn");

        if (voucher.Quantity.HasValue && voucher.QuantityRedeemed >= voucher.Quantity.Value)
            return (false, "Voucher đã hết số lượng");

        var memberPoint = await _memberPointService.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null) return (false, "Không tìm thấy thông tin hội viên");

        if (memberPoint.Points < voucher.PointsRequired)
            return (false, $"Bạn cần {voucher.PointsRequired} điểm để đổi voucher này (bạn có {memberPoint.Points} điểm)");

        // Check tier
        if (voucher.MinTier.HasValue)
        {
            var tierOrder = new Dictionary<string, int>
            {
                { "Bronze", 1 },
                { "Silver", 2 },
                { "Gold", 3 },
                { "Diamond", 4 }
            };

            if (!tierOrder.ContainsKey(memberPoint.Tier) || 
                !tierOrder.ContainsKey(voucher.MinTier.ToString()))
            {
                return (false, "Không thể xác định cấp bậc hội viên");
            }

            if (tierOrder[memberPoint.Tier] < tierOrder[voucher.MinTier.ToString()])
                return (false, $"Cần cấp bậc tối thiểu {voucher.MinTier} để đổi voucher này");
        }

        return (true, null);
    }

    private (bool CanRedeem, string? Reason) CanUserRedeemVoucher(Voucher voucher, MemberPointResponseDto memberPoint)
    {
        // Check tier
        if (voucher.MinTier.HasValue)
        {
            var tierOrder = new Dictionary<string, int>
            {
                { "Bronze", 1 },
                { "Silver", 2 },
                { "Gold", 3 },
                { "Diamond", 4 }
            };

            if (!tierOrder.ContainsKey(memberPoint.Tier) || 
                !tierOrder.ContainsKey(voucher.MinTier.ToString()))
            {
                return (false, "Không thể xác định cấp bậc");
            }

            if (tierOrder[memberPoint.Tier] < tierOrder[voucher.MinTier.ToString()])
                return (false, $"Cần cấp {voucher.MinTier}");
        }

        // Check points
        if (memberPoint.Points < voucher.PointsRequired)
            return (false, $"Cần {voucher.PointsRequired} điểm");

        // Check quantity
        if (voucher.Quantity.HasValue && voucher.QuantityRedeemed >= voucher.Quantity.Value)
            return (false, "Đã hết");

        return (true, null);
    }

    private static VoucherResponseDto MapToResponseDto(Voucher voucher)
    {
        return new VoucherResponseDto
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            Description = voucher.Description,
            ImageUrl = voucher.ImageUrl,
            Type = voucher.Type.ToString(),
            DiscountValue = voucher.DiscountValue,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            MinOrderAmount = voucher.MinOrderAmount,
            PointsRequired = voucher.PointsRequired,
            Quantity = voucher.Quantity,
            QuantityRedeemed = voucher.QuantityRedeemed,
            RemainingQuantity = voucher.Quantity.HasValue 
                ? Math.Max(0, voucher.Quantity.Value - voucher.QuantityRedeemed)
                : null,
            ValidFrom = voucher.ValidFrom,
            ValidTo = voucher.ValidTo,
            Status = voucher.Status.ToString(),
            MinTier = voucher.MinTier?.ToString(),
            CreatedAt = voucher.CreatedAt,
            UpdatedAt = voucher.UpdatedAt
        };
    }
}
