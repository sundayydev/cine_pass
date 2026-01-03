using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services.BackgroundJobs;

public class BirthdayVoucherJob
{
    // private readonly ApplicationDbContext _context;
    // private readonly IEventBus _eventBus;
    // private readonly ILogger<BirthdayVoucherJob> _logger;
    //
    // public BirthdayVoucherJob(
    //     ApplicationDbContext context,
    //     IEventBus eventBus,
    //     ILogger<BirthdayVoucherJob> logger)
    // {
    //     _context = context;
    //     _eventBus = eventBus;
    //     _logger = logger;
    // }
    //
    // /// <summary>
    // /// Check for users with birthdays today and send vouchers
    // /// </summary>
    // public async Task SendBirthdayVouchersAsync(CancellationToken cancellationToken = default)
    // {
    //     _logger.LogInformation("Starting birthday voucher job");
    //
    //     try
    //     {
    //         var today = DateTime.UtcNow;
    //         
    //         // Get users with birthday today (adjust based on your actual User entity)
    //         var birthdayUsers = await _context.Set<Domain.Models.User>()
    //             .Where(u => u.DateOfBirth.HasValue)
    //             .Where(u => u.DateOfBirth.Value.Month == today.Month && u.DateOfBirth.Value.Day == today.Day)
    //             .ToListAsync(cancellationToken);
    //
    //         _logger.LogInformation(
    //             "Found {Count} users with birthday today", 
    //             birthdayUsers.Count);
    //
    //         foreach (var user in birthdayUsers)
    //         {
    //             // Generate or assign birthday voucher (implement your logic)
    //             var voucherCode = $"BIRTHDAY{today.Year}{user.Id.ToString().Substring(0, 8).ToUpper()}";
    //
    //             // Publish event
    //             await _eventBus.PublishAsync(new Domain.Events.BirthdayVoucherEvent
    //             {
    //                 UserId = user.Id,
    //                 UserName = user.FullName ?? user.Email,
    //                 VoucherCode = voucherCode
    //             }, cancellationToken);
    //         }
    //
    //         _logger.LogInformation(
    //             "Completed birthday voucher job, sent {Count} vouchers", 
    //             birthdayUsers.Count);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error in birthday voucher job");
    //         throw;
    //     }
    // }
}








