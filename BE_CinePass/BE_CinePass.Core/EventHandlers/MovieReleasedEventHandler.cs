using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class MovieReleasedEventHandler : IEventHandler<MovieReleasedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<MovieReleasedEventHandler> _logger;

    public MovieReleasedEventHandler(
        ApplicationDbContext context,
        NotificationHelperService notificationHelper,
        ILogger<MovieReleasedEventHandler> logger)
    {
        _context = context;
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(MovieReleasedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling MovieReleasedEvent for Movie {MovieId} ({MovieTitle})", 
            @event.MovieId,
            @event.MovieTitle);

        try
        {
            // ✅ Get all users who subscribed to this movie
            var subscribedUsers = await _context.Set<MovieReminder>()
                .Where(mr => mr.MovieId == @event.MovieId)
                .Where(mr => mr.IsActive)
                .Where(mr => mr.NotifiedAt == null) // Not yet notified
                .Select(mr => mr.UserId)
                .ToListAsync(cancellationToken);

            if (!subscribedUsers.Any())
            {
                _logger.LogInformation(
                    "No subscribers found for Movie {MovieId}",
                    @event.MovieId);
                return;
            }

            _logger.LogInformation(
                "Found {Count} subscribers for Movie {MovieId}",
                subscribedUsers.Count,
                @event.MovieId);

            // Send notification to each subscribed user
            int successCount = 0;
            int failedCount = 0;

            foreach (var userId in subscribedUsers)
            {
                try
                {
                    await _notificationHelper.SendMovieReleasedNotificationAsync(
                        userId,
                        @event.MovieId,
                        @event.MovieTitle,
                        @event.PosterUrl,
                        @event.ReleaseDate,
                        cancellationToken);

                    successCount++;

                    // Mark as notified
                    var reminder = await _context.Set<MovieReminder>()
                        .FirstOrDefaultAsync(
                            mr => mr.MovieId == @event.MovieId && mr.UserId == userId,
                            cancellationToken);

                    if (reminder != null)
                    {
                        reminder.NotifiedAt = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send movie release notification to User {UserId}",
                        userId);
                    failedCount++;
                }
            }

            // Save notification timestamps
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MovieReleasedEvent processed for {MovieTitle}. Success: {Success}, Failed: {Failed}",
                @event.MovieTitle,
                successCount,
                failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MovieReleasedEvent for Movie {MovieId}", @event.MovieId);
            throw;
        }
    }
}