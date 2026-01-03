// BE_CinePass/BE_CinePass.Core/Repositories/NotificationSettingsRepository.cs
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;


public class NotificationSettingsRepository : BaseRepository<NotificationSettings>, INotificationSettingsRepository
{
    public NotificationSettingsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<NotificationSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ns => ns.UserId == userId, cancellationToken);
    }

    public async Task<NotificationSettings> CreateDefaultAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = new NotificationSettings
        {
            UserId = userId,
            EnableUpcomingShowtime = true,
            EnableOrderStatus = true,
            EnablePromotion = true,
            EnableSystem = true,
            ShowtimeReminderMinutes = 120,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await AddAsync(settings, cancellationToken);
        return settings;
    }
}