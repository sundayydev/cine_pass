using BE_CinePass.Core.Services.BackgroundJobs;
using BE_CinePass.Domain.Events;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class ShowtimeCreatedEventHandler : IEventHandler<ShowtimeCreatedEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<ShowtimeCreatedEventHandler> _logger;

    public ShowtimeCreatedEventHandler(
        IBackgroundJobClient backgroundJobClient,
        ILogger<ShowtimeCreatedEventHandler> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public Task HandleAsync(ShowtimeCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling ShowtimeCreatedEvent for Showtime {ShowtimeId} - scheduling reminders", 
            @event.ShowtimeId);

        // FIX: Ensure StartTime is treated as UTC (no conversion needed)
        // var startTimeUtc = @event.StartTime.Kind == DateTimeKind.Utc 
        //     ? @event.StartTime 
        //     : DateTime.SpecifyKind(@event.StartTime, DateTimeKind.Utc);
        // var startTimeUtc = @event.StartTime;
        var startTimeUtc =
            @event.StartTime.Kind == DateTimeKind.Utc
                ? @event.StartTime
                : DateTime.SpecifyKind(@event.StartTime, DateTimeKind.Local)
                    .ToUniversalTime();
        _logger.LogInformation(
            "StartTime (UTC): {StartTime}, Current (UTC): {Now}",
            startTimeUtc,
            DateTime.UtcNow);

        // Schedule reminder notifications
        // 60 minutes before
        var reminder60Min = startTimeUtc.AddMinutes(-60);
        if (reminder60Min > DateTime.UtcNow)
        {
            var scheduleTime = new DateTimeOffset(reminder60Min, TimeSpan.Zero);
            
            _backgroundJobClient.Schedule<ShowtimeReminderJob>(
                job => job.SendShowtimeRemindersAsync(@event.ShowtimeId, cancellationToken),
                scheduleTime);
            
            _logger.LogInformation(
                "Scheduled 60-min reminder for Showtime {ShowtimeId} at {Time} UTC (in {Minutes} minutes)",
                @event.ShowtimeId, 
                reminder60Min,
                (reminder60Min - DateTime.UtcNow).TotalMinutes);
        }
        else
        {
            _logger.LogWarning(
                "Cannot schedule 60-min reminder: time {Time} is in the past (now: {Now}). " +
                "Showtime is too soon or already started.",
                reminder60Min,
                DateTime.UtcNow);
        }

        // 30 minutes before
        var reminder30Min = startTimeUtc.AddMinutes(-30);
        if (reminder30Min > DateTime.UtcNow)
        {
            var scheduleTime = new DateTimeOffset(reminder30Min, TimeSpan.Zero);
            
            _backgroundJobClient.Schedule<ShowtimeReminderJob>(
                job => job.SendShowtimeRemindersAsync(@event.ShowtimeId, cancellationToken),
                scheduleTime);
            
            _logger.LogInformation(
                "Scheduled 30-min reminder for Showtime {ShowtimeId} at {Time} UTC (in {Minutes} minutes)",
                @event.ShowtimeId, 
                reminder30Min,
                (reminder30Min - DateTime.UtcNow).TotalMinutes);
        }
        else
        {
            _logger.LogWarning(
                "Cannot schedule 30-min reminder: time {Time} is in the past (now: {Now})",
                reminder30Min,
                DateTime.UtcNow);
        }

        // 15 minutes before
        var reminder15Min = startTimeUtc.AddMinutes(-15);
        if (reminder15Min > DateTime.UtcNow)
        {
            var scheduleTime = new DateTimeOffset(reminder15Min, TimeSpan.Zero);
            
            _backgroundJobClient.Schedule<ShowtimeReminderJob>(
                job => job.SendShowtimeRemindersAsync(@event.ShowtimeId, cancellationToken),
                scheduleTime);
            
            _logger.LogInformation(
                "Scheduled 15-min reminder for Showtime {ShowtimeId} at {Time} UTC (in {Minutes} minutes)",
                @event.ShowtimeId, 
                reminder15Min,
                (reminder15Min - DateTime.UtcNow).TotalMinutes);
        }
        else
        {
            _logger.LogWarning(
                "Cannot schedule 15-min reminder: time {Time} is in the past (now: {Now})",
                reminder15Min,
                DateTime.UtcNow);
        }

        return Task.CompletedTask;
    }
}