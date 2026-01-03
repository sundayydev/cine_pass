using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services.BackgroundJobs;

public class ShowtimeReminderJob
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<ShowtimeReminderJob> _logger;

    public ShowtimeReminderJob(
        ApplicationDbContext context,
        NotificationHelperService notificationHelper,
        ILogger<ShowtimeReminderJob> logger)
    {
        _context = context;
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    /// <summary>
    /// Send reminder notifications to all users who booked this showtime
    /// </summary>
    public async Task SendShowtimeRemindersAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting showtime reminder job for Showtime {ShowtimeId}", showtimeId);

        try
        {
            // Get showtime details
            var showtime = await _context.Set<Domain.Models.Showtime>()
                .Include(s => s.Movie)
                .Include(s => s.Screen)
                    .ThenInclude(sc => sc.Cinema)
                .FirstOrDefaultAsync(s => s.Id == showtimeId, cancellationToken);

            if (showtime == null)
            {
                _logger.LogWarning("Showtime {ShowtimeId} not found", showtimeId);
                return;
            }

            _logger.LogInformation(
                "Processing reminder for: Movie '{Movie}' at {Cinema} - {Screen}, Start: {StartTime}",
                showtime.Movie.Title,
                showtime.Screen.Cinema?.Name ?? "Unknown Cinema",
                showtime.Screen.Name,
                showtime.StartTime);

            // ✅ FIX: Get all orders that have tickets for this showtime
            var ordersWithShowtime = await _context.Set<Domain.Models.OrderTicket>()
                .Include(ot => ot.Order)
                    .ThenInclude(o => o.User)
                .Where(ot => ot.ShowtimeId == showtimeId)
                .Where(ot => ot.Order.Status == OrderStatus.Confirmed)
                .Select(ot => new
                {
                    UserId = ot.Order.UserId,
                    OrderId = ot.Order.Id,
                    UserName = ot.Order.User != null ? ot.Order.User.FullName : null
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!ordersWithShowtime.Any())
            {
                _logger.LogInformation(
                    "No confirmed orders found for Showtime {ShowtimeId}", 
                    showtimeId);
                return;
            }

            _logger.LogInformation(
                "Found {Count} users with confirmed orders for Showtime {ShowtimeId}", 
                ordersWithShowtime.Count, 
                showtimeId);

            // Send reminder to each user
            int successCount = 0;
            int failedCount = 0;

            foreach (var orderInfo in ordersWithShowtime)
            {
                if (!orderInfo.UserId.HasValue)
                {
                    _logger.LogWarning("Order {OrderId} has no UserId, skipping", orderInfo.OrderId);
                    failedCount++;
                    continue;
                }

                try
                {
                    await _notificationHelper.SendShowtimeReminderNotificationAsync(
                        orderInfo.UserId.Value,
                        showtimeId,
                        showtime.Movie.Title,
                        $"{showtime.Screen.Cinema?.Name ?? "Cinema"} - {showtime.Screen.Name}",
                        showtime.StartTime,
                        cancellationToken);

                    successCount++;

                    _logger.LogInformation(
                        "Sent reminder to User {UserId} ({UserName}) for Showtime {ShowtimeId}",
                        orderInfo.UserId.Value,
                        orderInfo.UserName ?? "Unknown",
                        showtimeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send reminder to User {UserId} for Showtime {ShowtimeId}",
                        orderInfo.UserId.Value,
                        showtimeId);
                    failedCount++;
                }
            }

            _logger.LogInformation(
                "Completed showtime reminder job for Showtime {ShowtimeId}. Success: {Success}, Failed: {Failed}",
                showtimeId,
                successCount,
                failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in showtime reminder job for Showtime {ShowtimeId}", showtimeId);
            throw;
        }
    }
}