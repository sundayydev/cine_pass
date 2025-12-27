using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.MemberTierConfig;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BE_CinePass.Core.Services;

public class MemberTierConfigService
{
    private readonly ApplicationDbContext _context;

    public MemberTierConfigService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MemberTierConfigResponseDto>> GetAllTiersAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _context.MemberTierConfigs
            .OrderBy(c => c.MinPoints)
            .ToListAsync(cancellationToken);

        return configs.Select(MapToResponseDto).ToList();
    }

    public async Task<MemberTierConfigResponseDto?> GetByTierAsync(MemberTier tier, CancellationToken cancellationToken = default)
    {
        var config = await _context.MemberTierConfigs
            .FirstOrDefaultAsync(c => c.Tier == tier, cancellationToken);

        return config == null ? null : MapToResponseDto(config);
    }

    public async Task<MemberTierConfigResponseDto?> GetTierByNameAsync(MemberTier tier, CancellationToken cancellationToken = default)
    {
        var config = await _context.MemberTierConfigs
            .FirstOrDefaultAsync(c => c.Tier == tier, cancellationToken);

        return config == null ? null : MapToResponseDto(config);
    }

    public async Task<MemberTier> DetermineTierByLifetimePointsAsync(int lifetimePoints, CancellationToken cancellationToken = default)
    {
        var tier = await _context.MemberTierConfigs
            .Where(c => lifetimePoints >= c.MinPoints 
                     && (c.MaxPoints == null || lifetimePoints <= c.MaxPoints))
            .OrderByDescending(c => c.MinPoints)
            .Select(c => c.Tier)
            .FirstOrDefaultAsync(cancellationToken);

        return tier != default ? tier : MemberTier.Bronze;
    }

    public async Task<MemberTierConfig> CreateTierConfigAsync(MemberTierConfigCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Check if tier already exists
        var tierEnum = Enum.Parse<MemberTier>(dto.Tier);
        if (await _context.MemberTierConfigs.AnyAsync(t => t.Tier == tierEnum, cancellationToken))
            throw new InvalidOperationException($"Tier config '{dto.Tier}' already exists");

        var config = new MemberTierConfig
        {
            Tier = tierEnum,
            Name = dto.Name,
            MinPoints = dto.MinPoints,
            MaxPoints = dto.MaxPoints,
            PointMultiplier = dto.PointMultiplier,
            DiscountPercentage = dto.DiscountPercentage,
            Color = dto.Color,
            IconUrl = dto.IconUrl,
            Description = dto.Description,
            Benefits = dto.Benefits != null ? JsonSerializer.Serialize(dto.Benefits) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MemberTierConfigs.Add(config);
        await _context.SaveChangesAsync(cancellationToken);

        return config;
    }

    public async Task<bool> UpdateTierConfigAsync(Guid id, MemberTierConfigUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var config = await _context.MemberTierConfigs.FindAsync(new object[] { id }, cancellationToken);
        if (config == null) return false;

        if (dto.Name != null) config.Name = dto.Name;
        if (dto.MinPoints.HasValue) config.MinPoints = dto.MinPoints.Value;
        if (dto.MaxPoints.HasValue) config.MaxPoints = dto.MaxPoints.Value;
        if (dto.PointMultiplier.HasValue) config.PointMultiplier = dto.PointMultiplier.Value;
        if (dto.DiscountPercentage.HasValue) config.DiscountPercentage = dto.DiscountPercentage.Value;
        if (dto.Color != null) config.Color = dto.Color;
        if (dto.IconUrl != null) config.IconUrl = dto.IconUrl;
        if (dto.Description != null) config.Description = dto.Description;
        if (dto.Benefits != null) config.Benefits = JsonSerializer.Serialize(dto.Benefits);

        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteTierConfigAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await _context.MemberTierConfigs.FindAsync(new object[] { id }, cancellationToken);
        if (config == null) return false;

        // Check if any users are using this tier
        var usersWithTier = await _context.MemberPoints
            .AnyAsync(mp => mp.Tier == config.Tier, cancellationToken);

        if (usersWithTier)
            throw new InvalidOperationException("Cannot delete tier config that is currently assigned to users");

        _context.MemberTierConfigs.Remove(config);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static MemberTierConfigResponseDto MapToResponseDto(MemberTierConfig config)
    {
        List<string>? benefits = null;
        if (!string.IsNullOrEmpty(config.Benefits))
        {
            try
            {
                benefits = System.Text.Json.JsonSerializer.Deserialize<List<string>>(config.Benefits);
            }
            catch { }
        }

        return new MemberTierConfigResponseDto
        {
            Id = config.Id,
            Tier = config.Tier.ToString(),
            Name = config.Name,
            MinPoints = config.MinPoints,
            MaxPoints = config.MaxPoints,
            PointMultiplier = config.PointMultiplier,
            DiscountPercentage = config.DiscountPercentage,
            Color = config.Color,
            IconUrl = config.IconUrl,
            Description = config.Description,
            Benefits = benefits,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };
    }
}
