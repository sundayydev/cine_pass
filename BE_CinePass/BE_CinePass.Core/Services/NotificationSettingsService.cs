using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Shared.DTOs.Notification;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services;

public class NotificationSettingsService
{
    private readonly NotificationSettingsRepository _settingsRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationSettingsService> _logger;

    public NotificationSettingsService(
        NotificationSettingsRepository settingsRepository,
        ApplicationDbContext context,
        ILogger<NotificationSettingsService> logger)
    {
        _settingsRepository = settingsRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<NotificationSettingsResponseDto> GetOrCreateSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetByUserIdAsync(userId, cancellationToken);

        if (settings == null)
        {
            settings = await _settingsRepository.CreateDefaultAsync(userId, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created default notification settings for user {UserId}", userId);
        }

        return MapToResponseDto(settings);
    }

    public async Task<NotificationSettingsResponseDto> UpdateSettingsAsync(
        Guid userId,
        UpdateNotificationSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetByUserIdAsync(userId, cancellationToken);

        if (settings == null)
        {
            settings = await _settingsRepository.CreateDefaultAsync(userId, cancellationToken);
        }

        // Update fields
        if (dto.EnableUpcomingShowtime.HasValue)
            settings.EnableUpcomingShowtime = dto.EnableUpcomingShowtime.Value;

        if (dto.EnableOrderStatus.HasValue)
            settings.EnableOrderStatus = dto.EnableOrderStatus.Value;

        if (dto.EnablePromotion.HasValue)
            settings.EnablePromotion = dto.EnablePromotion.Value;

        if (dto.EnableSystem.HasValue)
            settings.EnableSystem = dto.EnableSystem.Value;

        if (dto.ShowtimeReminderMinutes.HasValue)
            settings.ShowtimeReminderMinutes = dto.ShowtimeReminderMinutes.Value;

        settings.UpdatedAt = DateTime.UtcNow;

        _settingsRepository.Update(settings);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated notification settings for user {UserId}", userId);

        return MapToResponseDto(settings);
    }

    private static NotificationSettingsResponseDto MapToResponseDto(Domain.Models.NotificationSettings settings)
    {
        return new NotificationSettingsResponseDto
        {
            Id = settings.Id,
            UserId = settings.UserId,
            EnableUpcomingShowtime = settings.EnableUpcomingShowtime,
            EnableOrderStatus = settings.EnableOrderStatus,
            EnablePromotion = settings.EnablePromotion,
            EnableSystem = settings.EnableSystem,
            ShowtimeReminderMinutes = settings.ShowtimeReminderMinutes,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }
}