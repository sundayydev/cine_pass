using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BE_CinePass.Core.Services;

/// <summary>
/// Service for managing temporary seat holds using Redis
/// </summary>
public class SeatHoldService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<SeatHoldService> _logger;
    private const int HOLD_DURATION_MINUTES = 10; // Seats held for 10 minutes

    public SeatHoldService(IDistributedCache cache, ILogger<SeatHoldService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Hold seats for a user temporarily
    /// </summary>
    public async Task<bool> HoldSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(HOLD_DURATION_MINUTES)
            };

            foreach (var seatId in seatIds)
            {
                var key = GetSeatHoldKey(showtimeId, seatId);

                // Check if seat is already held
                var existingHold = await _cache.GetStringAsync(key, cancellationToken);
                if (!string.IsNullOrEmpty(existingHold))
                {
                    var existingUserId = existingHold;
                    if (existingUserId != userId)
                    {
                        _logger.LogWarning("Seat {SeatId} is already held by user {UserId}", seatId, existingUserId);
                        return false; // Seat already held by another user
                    }
                }

                // Hold the seat
                await _cache.SetStringAsync(key, userId, options, cancellationToken);
            }

            _logger.LogInformation("User {UserId} held {Count} seats for showtime {ShowtimeId}",
                userId, seatIds.Count, showtimeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error holding seats for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Release held seats
    /// </summary>
    public async Task ReleaseSeatsAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var seatId in seatIds)
            {
                var key = GetSeatHoldKey(showtimeId, seatId);
                await _cache.RemoveAsync(key, cancellationToken);
            }

            _logger.LogInformation("Released {Count} seats for showtime {ShowtimeId}",
                seatIds.Count, showtimeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing seats");
        }
    }

    /// <summary>
    /// Get the user ID holding a specific seat
    /// </summary>
    public async Task<string?> GetSeatHolderAsync(Guid showtimeId, Guid seatId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetSeatHoldKey(showtimeId, seatId);
            return await _cache.GetStringAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seat holder for seat {SeatId}", seatId);
            return null;
        }
    }

    /// <summary>
    /// Check if a seat is currently held
    /// </summary>
    public async Task<bool> IsSeatHeldAsync(Guid showtimeId, Guid seatId, CancellationToken cancellationToken = default)
    {
        var holder = await GetSeatHolderAsync(showtimeId, seatId, cancellationToken);
        return !string.IsNullOrEmpty(holder);
    }

    /// <summary>
    /// Get all held seats for a showtime with their holders
    /// </summary>
    public async Task<Dictionary<Guid, string>> GetHeldSeatsAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        var heldSeats = new Dictionary<Guid, string>();

        foreach (var seatId in seatIds)
        {
            var holder = await GetSeatHolderAsync(showtimeId, seatId, cancellationToken);
            if (!string.IsNullOrEmpty(holder))
            {
                heldSeats[seatId] = holder;
            }
        }

        return heldSeats;
    }

    private static string GetSeatHoldKey(Guid showtimeId, Guid seatId)
    {
        return $"seat_hold:{showtimeId}:{seatId}";
    }
}
