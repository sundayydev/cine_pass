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
    /// Check for vouchers expiring soon (within 3 days)
    /// </summary>
    public async Task CheckVoucherExpiryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting voucher expiry check job");

        try
        {
            var now = DateTime.UtcNow;
            var start = now.Date.AddDays(3);
            var end = start.AddDays(1);

            var expiringVouchers = await _context.Set<UserVoucher>()
                .Include(uv => uv.Voucher)
                .Where(uv => !uv.IsUsed)
                .Where(uv => uv.ExpiresAt.HasValue)
                .Where(uv =>
                    uv.ExpiresAt.Value >= start &&
                    uv.ExpiresAt.Value < end
                )
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Found {Count} vouchers expiring soon", 
                expiringVouchers.Count);

            foreach (var userVoucher in expiringVouchers)
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
            }

            if (expiringVouchers.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Completed voucher expiry check job, sent {Count} reminders", 
                expiringVouchers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in voucher expiry check job");
            throw;
        }
    }
}