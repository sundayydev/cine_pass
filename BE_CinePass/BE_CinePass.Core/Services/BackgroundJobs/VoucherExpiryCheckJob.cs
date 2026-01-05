using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Events;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services.BackgroundJobs;

public class VoucherExpiryCheckJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ILogger<VoucherExpiryCheckJob> _logger;

    public VoucherExpiryCheckJob(
        ApplicationDbContext context,
        IEventBus eventBus,
        ILogger<VoucherExpiryCheckJob> logger)
    {
        _context = context;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <summary>
    /// Check for vouchers expiring at exactly 3 or 7 days from now
    /// </summary>
    public async Task CheckVoucherExpiryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting voucher expiry check job");

        try
        {
            var now = DateTime.UtcNow;
            int successCount = 0;
            int failedCount = 0;

            // Check for two specific timeframes: 3 days and 7 days
            var notificationDays = new[] { 3, 7 };

            foreach (var days in notificationDays)
            {
                var targetDate = now.Date.AddDays(days);
                var nextDate = targetDate.AddDays(1);

                var expiringVouchers = await _context.Set<UserVoucher>()
                    .Include(uv => uv.Voucher)
                    .Where(uv => !uv.IsUsed)
                    .Where(uv => uv.ExpiresAt.HasValue)
                    .Where(uv =>
                        uv.ExpiresAt.Value >= targetDate &&
                        uv.ExpiresAt.Value < nextDate
                    )
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Found {Count} vouchers expiring in exactly {Days} days",
                    expiringVouchers.Count,
                    days);

                foreach (var userVoucher in expiringVouchers)
                {
                    try
                    {
                        var daysUntilExpiry = (int)(userVoucher.ExpiresAt!.Value - now).TotalDays;

                        // Publish event
                        await _eventBus.PublishAsync(new Domain.Events.VoucherExpiringSoonEvent
                        {
                            UserId = userVoucher.UserId,
                            VoucherCode = userVoucher.Voucher.Code,
                            VoucherName = userVoucher.Voucher.Name,
                            ExpiresAt = userVoucher.ExpiresAt.Value,
                            DaysUntilExpiry = daysUntilExpiry
                        }, cancellationToken);

                        successCount++;

                        _logger.LogInformation(
                            "Sent expiry notification for voucher {Code} to user {UserId} ({Days} days until expiry)",
                            userVoucher.Voucher.Code,
                            userVoucher.UserId,
                            daysUntilExpiry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to send expiry notification for voucher {Code}",
                            userVoucher.Voucher.Code);
                        failedCount++;
                    }
                }
            }

            _logger.LogInformation(
                "Completed voucher expiry check job. Success: {Success}, Failed: {Failed}",
                successCount,
                failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in voucher expiry check job");
            throw;
        }
    }
}