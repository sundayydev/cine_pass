using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.MemberPoint;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class MemberPointService
{
    private readonly MemberPointRepository _memberPointRepository;
    private readonly ApplicationDbContext _context;
    private readonly MemberTierConfigService _tierConfigService;

    public MemberPointService(
        MemberPointRepository memberPointRepository, 
        ApplicationDbContext context,
        MemberTierConfigService tierConfigService)
    {
        _memberPointRepository = memberPointRepository;
        _context = context;
        _tierConfigService = tierConfigService;
    }

    /// <summary>
    /// Get member point profile with full tier information
    /// </summary>
    public async Task<MemberPointResponseDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null) return null;

        return await MapToResponseDtoWithTierInfoAsync(memberPoint, cancellationToken);
    }

    public async Task<MemberPointResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByIdAsync(id, cancellationToken);
        if (memberPoint == null) return null;
        
        return await MapToResponseDtoWithTierInfoAsync(memberPoint, cancellationToken);
    }

    public async Task<MemberPointResponseDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null) return null;
        
        return await MapToResponseDtoWithTierInfoAsync(memberPoint, cancellationToken);
    }

    public async Task<List<MemberPointResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var memberPoints = await _memberPointRepository.GetAllAsync(cancellationToken);
        var result = new List<MemberPointResponseDto>();
        
        foreach (var mp in memberPoints)
        {
            result.Add(await MapToResponseDtoWithTierInfoAsync(mp, cancellationToken));
        }
        
        return result;
    }

    public async Task<MemberPointResponseDto> CreateAsync(Guid userId, int initialPoints = 0, CancellationToken cancellationToken = default)
    {
        // Check if user already has member points
        if (await _memberPointRepository.ExistsAsync(userId, cancellationToken))
            throw new InvalidOperationException($"User {userId} already has member points");

        var memberPoint = new MemberPoint
        {
            UserId = userId,
            Points = initialPoints,
            LifetimePoints = initialPoints,
            Tier = MemberTier.Bronze,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _memberPointRepository.AddAsync(memberPoint, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapToResponseDtoWithTierInfoAsync(memberPoint, cancellationToken);
    }

    /// <summary>
    /// Add points and lifetime points, automatically upgrade tier if needed
    /// </summary>
    public async Task<MemberPointResponseDto?> AddPointsAsync(
        Guid userId, 
        int points, 
        bool addToLifetime = true,
        CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null)
            return null;

        memberPoint.Points += points;
        
        if (addToLifetime)
        {
            memberPoint.LifetimePoints += points;
            // Check for tier upgrade
            await UpdateMemberTierAsync(memberPoint, cancellationToken);
        }
        
        memberPoint.UpdatedAt = DateTime.UtcNow;

        _memberPointRepository.Update(memberPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapToResponseDtoWithTierInfoAsync(memberPoint, cancellationToken);
    }

    /// <summary>
    /// Add points from order with tier-based multiplier
    /// </summary>
    public async Task<(int BasePoints, int BonusPoints, int TotalPoints)> AddPointsFromOrderAsync(
        Guid userId,
        decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null)
            throw new InvalidOperationException("Member point not found for user");

        var tierConfig = await _tierConfigService.GetByTierAsync(memberPoint.Tier, cancellationToken);
        if (tierConfig == null)
            throw new InvalidOperationException($"Tier configuration not found for {memberPoint.Tier}");

        // Calculate points: (orderAmount / 1000) * multiplier
        var basePoints = (int)(orderAmount / 1000);
        var multiplier = tierConfig.PointMultiplier;
        var totalPoints = (int)(basePoints * multiplier);
        var bonusPoints = totalPoints - basePoints;

        memberPoint.Points += totalPoints;
        memberPoint.LifetimePoints += totalPoints;
        memberPoint.UpdatedAt = DateTime.UtcNow;

        // Check for tier upgrade
        await UpdateMemberTierAsync(memberPoint, cancellationToken);

        _memberPointRepository.Update(memberPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return (basePoints, bonusPoints, totalPoints);
    }

    public async Task<MemberPointResponseDto?> SubtractPointsAsync(Guid userId, int points, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null)
            return null;

        if (memberPoint.Points < points)
            throw new InvalidOperationException("Insufficient points");

        memberPoint.Points -= points;
        // Note: LifetimePoints is NOT reduced when redeeming vouchers
        memberPoint.UpdatedAt = DateTime.UtcNow;

        _memberPointRepository.Update(memberPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapToResponseDtoWithTierInfoAsync(memberPoint, cancellationToken);
    }

    /// <summary>
    /// Automatically update member tier based on lifetime points
    /// </summary>
    private async Task UpdateMemberTierAsync(MemberPoint memberPoint, CancellationToken cancellationToken)
    {
        var allTiers = await _tierConfigService.GetAllTiersAsync(cancellationToken);
        
        var newTierConfig = allTiers
            .Where(t => memberPoint.LifetimePoints >= t.MinPoints &&
                       (t.MaxPoints == null || memberPoint.LifetimePoints <= t.MaxPoints))
            .OrderByDescending(t => t.MinPoints)
            .FirstOrDefault();

        if (newTierConfig != null && newTierConfig.Tier != memberPoint.Tier.ToString())
        {
            var oldTier = memberPoint.Tier;
            memberPoint.Tier = Enum.Parse<MemberTier>(newTierConfig.Tier);
            
            // TODO: Send notification about tier upgrade
            Console.WriteLine($"User tier upgraded from {oldTier} to {memberPoint.Tier}");
        }
    }

    private async Task<MemberPointResponseDto> MapToResponseDtoWithTierInfoAsync(
        MemberPoint memberPoint, 
        CancellationToken cancellationToken)
    {
        var tierConfig = await _tierConfigService.GetByTierAsync(memberPoint.Tier, cancellationToken);
        
        // Calculate points to next tier
        int? pointsToNextTier = null;
        string? nextTier = null;
        string? nextTierName = null;

        if (tierConfig?.MaxPoints != null)
        {
            var allTiers = await _tierConfigService.GetAllTiersAsync(cancellationToken);
            var nextTierConfig = allTiers
                .Where(t => t.MinPoints > memberPoint.LifetimePoints)
                .OrderBy(t => t.MinPoints)
                .FirstOrDefault();

            if (nextTierConfig != null)
            {
                pointsToNextTier = nextTierConfig.MinPoints - memberPoint.LifetimePoints;
                nextTier = nextTierConfig.Tier;
                nextTierName = nextTierConfig.Name;
            }
        }

        return new MemberPointResponseDto
        {
            Id = memberPoint.Id,
            UserId = memberPoint.UserId,
            Points = memberPoint.Points,
            LifetimePoints = memberPoint.LifetimePoints,
            Tier = memberPoint.Tier.ToString(),
            TierName = tierConfig?.Name ?? memberPoint.Tier.ToString(),
            PointsToExpire = memberPoint.PointsToExpire,
            NextExpiryDate = memberPoint.NextExpiryDate,
            CreatedAt = memberPoint.CreatedAt,
            UpdatedAt = memberPoint.UpdatedAt,
            PointMultiplier = tierConfig?.PointMultiplier ?? 1.0m,
            DiscountPercentage = tierConfig?.DiscountPercentage ?? 0m,
            TierColor = tierConfig?.Color,
            PointsToNextTier = pointsToNextTier,
            NextTier = nextTier,
            NextTierName = nextTierName,
            UserEmail = memberPoint.User?.Email,
            UserFullName = memberPoint.User?.FullName
        };
    }
}
