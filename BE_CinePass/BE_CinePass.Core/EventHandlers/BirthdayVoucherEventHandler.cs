using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class BirthdayVoucherEventHandler : IEventHandler<BirthdayVoucherEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<BirthdayVoucherEventHandler> _logger;

    public BirthdayVoucherEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<BirthdayVoucherEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(BirthdayVoucherEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling BirthdayVoucherEvent for User {UserId}", 
            @event.UserId);

        await _notificationHelper.SendBirthdayVoucherNotificationAsync(
            @event.UserId,
            @event.UserName,
            @event.VoucherCode,
            cancellationToken);
    }
}