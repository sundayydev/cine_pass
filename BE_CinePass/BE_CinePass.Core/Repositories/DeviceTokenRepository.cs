using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class DeviceTokenRepository : BaseRepository<DeviceToken>
{
    public DeviceTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all active device tokens for a user
    /// </summary>
    public async Task<List<DeviceToken>> GetActiveByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(dt => dt.UserId == userId && dt.IsActive)
            .OrderByDescending(dt => dt.LastUsedAt ?? dt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get all active device tokens for a specific platform
    /// </summary>
    public async Task<List<DeviceToken>> GetActiveByPlatformAsync(
        string platform, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(dt => dt.Platform == platform && dt.IsActive)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find device token by token string and user
    /// </summary>
    public async Task<DeviceToken?> FindByTokenAndUserAsync(
        string token, 
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                dt => dt.Token == token && dt.UserId == userId, 
                cancellationToken);
    }

    /// <summary>
    /// Find device token by token string (any user)
    /// </summary>
    public async Task<DeviceToken?> FindByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(dt => dt.Token == token, cancellationToken);
    }

    /// <summary>
    /// Deactivate (soft delete) a device token
    /// </summary>
    public async Task<bool> DeactivateAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        var token = await GetByIdAsync(id, cancellationToken);
        if (token == null)
            return false;

        token.IsActive = false;
        token.UpdatedAt = DateTime.UtcNow;
        
        Update(token);
        return true;
    }

    /// <summary>
    /// Deactivate all tokens for a user (on logout)
    /// </summary>
    public async Task<int> DeactivateAllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet
            .Where(dt => dt.UserId == userId && dt.IsActive)
            .ToListAsync(cancellationToken);

        if (!tokens.Any())
            return 0;

        foreach (var token in tokens)
        {
            token.IsActive = false;
            token.UpdatedAt = DateTime.UtcNow;
        }

        return tokens.Count;
    }

    /// <summary>
    /// Deactivate a specific token for a user
    /// </summary>
    public async Task<bool> DeactivateByTokenAsync(
        string token,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var deviceToken = await FindByTokenAndUserAsync(token, userId, cancellationToken);
        if (deviceToken == null)
            return false;

        deviceToken.IsActive = false;
        deviceToken.UpdatedAt = DateTime.UtcNow;
        
        Update(deviceToken);
        return true;
    }

    /// <summary>
    /// Update last used timestamp (after successful push)
    /// </summary>
    public async Task UpdateLastUsedAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var token = await GetByIdAsync(id, cancellationToken);
        if (token == null)
            return;

        token.LastUsedAt = DateTime.UtcNow;
        token.UpdatedAt = DateTime.UtcNow;
        
        Update(token);
    }

    /// <summary>
    /// Get all active tokens (for broadcast)
    /// </summary>
    public async Task<List<DeviceToken>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(dt => dt.IsActive)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Clean up old inactive tokens (maintenance)
    /// </summary>
    public async Task<int> DeleteInactiveOlderThanAsync(
        int days,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        var tokensToDelete = await _dbSet
            .Where(dt => !dt.IsActive && dt.UpdatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        if (!tokensToDelete.Any())
            return 0;

        _dbSet.RemoveRange(tokensToDelete);
        return tokensToDelete.Count;
    }

    /// <summary>
    /// Get token count statistics by platform
    /// </summary>
    public async Task<Dictionary<string, (int Total, int Active)>> GetStatsByPlatformAsync(
        CancellationToken cancellationToken = default)
    {
        var stats = await _dbSet
            .GroupBy(dt => dt.Platform)
            .Select(g => new
            {
                Platform = g.Key,
                Total = g.Count(),
                Active = g.Count(dt => dt.IsActive)
            })
            .ToListAsync(cancellationToken);

        return stats.ToDictionary(
            s => s.Platform,
            s => (s.Total, s.Active)
        );
    }

    /// <summary>
    /// Check if a token already exists for a user
    /// </summary>
    public async Task<bool> ExistsAsync(
        string token,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(dt => dt.Token == token && dt.UserId == userId, cancellationToken);
    }
}