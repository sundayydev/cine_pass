using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Notification;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services;

/// <summary>
/// Helper service for creating specific notification types with push notification support
/// </summary>
public class NotificationHelperService
{
    private readonly NotificationService _notificationService;
    private readonly PushNotificationService _pushNotificationService;
    private readonly ILogger<NotificationHelperService> _logger;

    public NotificationHelperService(
        NotificationService notificationService,
        PushNotificationService pushNotificationService,
        ILogger<NotificationHelperService> logger)
    {
        _notificationService = notificationService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    #region Order Notifications

    public async Task SendOrderConfirmedNotificationAsync(
        Guid userId,
        Guid orderId,
        string orderCode,
        decimal totalAmount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Đặt vé thành công";
            var message = $"Đơn hàng {orderCode} đã được xác nhận. Tổng tiền: {totalAmount:N0}đ";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenOrderDetail,
                Title = title,
                Message = message,
                Data = new { orderId, orderCode, totalAmount },
                ActionType = NotificationActionType.OpenOrderDetail,
                ActionData = orderId.ToString(),
                Priority = NotificationPriority.High
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "order_confirmed" },
                    { "orderId", orderId.ToString() },
                    { "orderCode", orderCode },
                    { "actionType", "OpenOrderDetail" },
                    { "actionData", orderId.ToString() }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order confirmed notification");
        }
    }

    public async Task SendOrderFailedNotificationAsync(
        Guid userId,
        Guid orderId,
        string orderCode,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Đặt vé thất bại";
            var message = $"Đơn hàng {orderCode} không thành công. {reason}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenOrderDetail,
                Title = title,
                Message = message,
                Data = new { orderId, orderCode, reason },
                ActionType = NotificationActionType.OpenOrderDetail,
                ActionData = orderId.ToString(),
                Priority = NotificationPriority.High
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "order_failed" },
                    { "orderId", orderId.ToString() },
                    { "orderCode", orderCode },
                    { "reason", reason },
                    { "actionType", "OpenOrderDetail" },
                    { "actionData", orderId.ToString() }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order failed notification");
        }
    }

    #endregion

    #region Payment Notifications

    public async Task SendPaymentSuccessNotificationAsync(
        Guid? userId,
        Guid orderId,
        string orderCode,
        decimal amount,
        string paymentMethod,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userId.HasValue) return;

            var title = "Thanh toán thành công";
            var message = $"Thanh toán {amount:N0}đ cho đơn hàng {orderCode} qua {paymentMethod} đã thành công";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenOrderDetail,
                Title = title,
                Message = message,
                Data = new { orderId, orderCode, amount, paymentMethod },
                ActionType = NotificationActionType.OpenOrderDetail,
                ActionData = orderId.ToString(),
                Priority = NotificationPriority.High
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId.Value,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "payment_success" },
                    { "orderId", orderId.ToString() },
                    { "orderCode", orderCode },
                    { "amount", amount.ToString() },
                    { "paymentMethod", paymentMethod },
                    { "actionType", "OpenOrderDetail" },
                    { "actionData", orderId.ToString() }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment success notification");
        }
    }

    public async Task SendPaymentFailedNotificationAsync(
        Guid? userId,
        Guid orderId,
        string orderCode,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userId.HasValue) return;

            var title = "Thanh toán thất bại";
            var message = $"Thanh toán cho đơn hàng {orderCode} không thành công. {reason}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenOrderDetail,
                Title = title,
                Message = message,
                Data = new { orderId, orderCode, reason },
                ActionType = NotificationActionType.OpenOrderDetail,
                ActionData = orderId.ToString(),
                Priority = NotificationPriority.High
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId.Value,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "payment_failed" },
                    { "orderId", orderId.ToString() },
                    { "orderCode", orderCode },
                    { "reason", reason },
                    { "actionType", "OpenOrderDetail" },
                    { "actionData", orderId.ToString() }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment failed notification");
        }
    }

    public async Task SendOrderRefundedNotificationAsync(
        Guid userId,
        Guid orderId,
        string orderCode,
        decimal refundAmount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Hoàn tiền thành công";
            var message = $"Đã hoàn {refundAmount:N0}đ cho đơn hàng {orderCode}. {reason}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenOrderDetail,
                Title = title,
                Message = message,
                Data = new { orderId, orderCode, refundAmount, reason },
                ActionType = NotificationActionType.OpenOrderDetail,
                ActionData = orderId.ToString(),
                Priority = NotificationPriority.High
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "order_refunded" },
                    { "orderId", orderId.ToString() },
                    { "orderCode", orderCode },
                    { "refundAmount", refundAmount.ToString() },
                    { "reason", reason },
                    { "actionType", "OpenOrderDetail" },
                    { "actionData", orderId.ToString() }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order refunded notification");
        }
    }

    #endregion

    #region Showtime Notifications

    public async Task SendShowtimeReminderNotificationAsync(
        Guid? userId,
        Guid showtimeId,
        string movieTitle,
        string cinemaName,
        DateTime startTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userId.HasValue) return;

            // startTime is already in UTC from database
            // Just ensure it's marked as UTC (if Kind is Unspecified)
            var startTimeUtc = startTime.Kind == DateTimeKind.Utc 
                ? startTime 
                : DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            
            var minutesUntil = (int)(startTimeUtc - DateTime.UtcNow).TotalMinutes;
            
            _logger.LogDebug(
                "Showtime reminder: startTime={StartTime}, startTimeUtc={StartTimeUtc}, currentUtc={CurrentUtc}, minutesUntil={Minutes}",
                startTime, startTimeUtc, DateTime.UtcNow, minutesUntil);

            var title = "Sắp tới giờ chiếu";
            var message = $"Phim '{movieTitle}' tại {cinemaName} sẽ bắt đầu sau {minutesUntil} phút";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenTicket,
                Title = title,
                Message = message,
                Data = new { showtimeId, movieTitle, cinemaName, startTime },
                ActionType = NotificationActionType.OpenShowtime,
                ActionData = showtimeId.ToString(),
                Priority = NotificationPriority.Urgent
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId.Value,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "showtime_reminder" },
                    { "showtimeId", showtimeId.ToString() },
                    { "movieTitle", movieTitle },
                    { "cinemaName", cinemaName },
                    { "startTime", startTime.ToString("o") },
                    { "minutesUntil", minutesUntil.ToString() },
                    { "actionType", "OpenShowtime" },
                    { "actionData", showtimeId.ToString() }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending showtime reminder notification");
        }
    }

    public async Task SendShowtimeCancelledNotificationAsync(
        Guid userId,
        string movieTitle,
        string cinemaName,
        DateTime startTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Suất chiếu bị hủy";
            var message = $"Rất tiếc, suất chiếu '{movieTitle}' tại {cinemaName} lúc {startTime:HH:mm dd/MM} đã bị hủy";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.None,
                Title = title,
                Message = message,
                Data = new { movieTitle, cinemaName, startTime },
                Priority = NotificationPriority.Urgent,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "showtime_cancelled" },
                    { "movieTitle", movieTitle },
                    { "cinemaName", cinemaName },
                    { "startTime", startTime.ToString("o") }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending showtime cancelled notification");
        }
    }

    public async Task SendShowtimeTimeChangedNotificationAsync(
        Guid userId,
        string movieTitle,
        string cinemaName,
        DateTime oldStartTime,
        DateTime newStartTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Thay đổi giờ chiếu";
            var message = $"Suất chiếu '{movieTitle}' tại {cinemaName} đã đổi từ {oldStartTime:HH:mm dd/MM} sang {newStartTime:HH:mm dd/MM}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenTicket,
                Title = title,
                Message = message,
                Data = new { movieTitle, cinemaName, oldStartTime, newStartTime },
                Priority = NotificationPriority.Urgent,
                ExpiresAt = newStartTime
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "showtime_time_changed" },
                    { "movieTitle", movieTitle },
                    { "cinemaName", cinemaName },
                    { "oldStartTime", oldStartTime.ToString("o") },
                    { "newStartTime", newStartTime.ToString("o") },
                    { "actionType", "OpenTicket" }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending showtime time changed notification");
        }
    }

    public async Task SendMovieReleasedNotificationAsync(
        Guid? userId,
        Guid movieId,
        string movieTitle,
        string? posterUrl,
        DateTime releaseDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userId.HasValue) return;

            var title = "Phim mới ra mắt";
            var message = $"Phim '{movieTitle}' đã chính thức công chiếu từ {releaseDate:dd/MM/yyyy}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenMovieDetail,
                Title = title,
                Message = message,
                Data = new { movieId, movieTitle, posterUrl, releaseDate },
                ImageUrl = posterUrl,
                ActionType = NotificationActionType.OpenMovieDetail,
                ActionData = movieId.ToString(),
                Priority = NotificationPriority.Normal,
                ExpiresAt = releaseDate.AddDays(7)
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId.Value,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "movie_released" },
                    { "movieId", movieId.ToString() },
                    { "movieTitle", movieTitle },
                    { "releaseDate", releaseDate.ToString("o") },
                    { "actionType", "OpenMovieDetail" },
                    { "actionData", movieId.ToString() }
                },
                imageUrl: posterUrl,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending movie released notification");
        }
    }

    #endregion

    #region Voucher Notifications

    public async Task SendVoucherReceivedNotificationAsync(
        Guid userId,
        string voucherCode,
        string voucherName,
        DateTime? expiresAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expiryText = expiresAt.HasValue 
                ? $"Hạn sử dụng: {expiresAt.Value:dd/MM/yyyy}" 
                : "";

            var title = "Bạn có voucher mới";
            var message = $"Voucher '{voucherName}' đã được thêm vào ví. {expiryText}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenVoucherList,
                Title = title,
                Message = message,
                Data = new { voucherCode, voucherName, expiresAt },
                ActionType = NotificationActionType.OpenVoucherList,
                Priority = NotificationPriority.Normal,
                ExpiresAt = expiresAt
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "voucher_received" },
                    { "voucherCode", voucherCode },
                    { "voucherName", voucherName },
                    { "expiresAt", expiresAt?.ToString("o") ?? "" },
                    { "actionType", "OpenVoucherList" }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending voucher received notification");
        }
    }

    public async Task SendVoucherExpiringSoonNotificationAsync(
        Guid userId,
        string voucherCode,
        string voucherName,
        DateTime expiresAt,
        int daysUntilExpiry,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Voucher sắp hết hạn";
            var message = $"Voucher '{voucherName}' sẽ hết hạn sau {daysUntilExpiry} ngày ({expiresAt:dd/MM/yyyy}). Hãy sử dụng ngay!";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenVoucherList,
                Title = title,
                Message = message,
                Data = new { voucherCode, voucherName, expiresAt, daysUntilExpiry },
                ActionType = NotificationActionType.OpenVoucherList,
                Priority = NotificationPriority.High,
                ExpiresAt = expiresAt
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "voucher_expiring_soon" },
                    { "voucherCode", voucherCode },
                    { "voucherName", voucherName },
                    { "expiresAt", expiresAt.ToString("o") },
                    { "daysUntilExpiry", daysUntilExpiry.ToString() },
                    { "actionType", "OpenVoucherList" }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending voucher expiring soon notification");
        }
    }

    public async Task SendBirthdayVoucherNotificationAsync(
        Guid userId,
        string userName,
        string voucherCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "🎂 Chúc mừng sinh nhật!";
            var message = $"Chúc mừng sinh nhật {userName}! Bạn nhận được voucher đặc biệt: {voucherCode}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenVoucherList,
                Title = title,
                Message = message,
                Data = new { userName, voucherCode },
                ActionType = NotificationActionType.OpenVoucherList,
                Priority = NotificationPriority.Normal
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "birthday_voucher" },
                    { "userName", userName },
                    { "voucherCode", voucherCode },
                    { "actionType", "OpenVoucherList" }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending birthday voucher notification");
        }
    }

    #endregion

    #region Points Notifications

    public async Task SendPointsEarnedNotificationAsync(
        Guid userId,
        int points,
        string source,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Cộng điểm thành công";
            var message = $"Bạn nhận được {points} điểm từ {source}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenUserProfile,
                Title = title,
                Message = message,
                Data = new { points, source },
                ActionType = NotificationActionType.OpenPointsHistory,
                Priority = NotificationPriority.Normal
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "points_earned" },
                    { "points", points.ToString() },
                    { "source", source },
                    { "actionType", "OpenPointsHistory" }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending points earned notification");
        }
    }

    #endregion

    #region System Notifications

    public async Task SendSystemMaintenanceNotificationAsync(
        DateTime maintenanceStart,
        DateTime maintenanceEnd,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "Thông báo bảo trì hệ thống";
            var message = $"Hệ thống sẽ bảo trì từ {maintenanceStart:HH:mm dd/MM} đến {maintenanceEnd:HH:mm dd/MM}";

            // 1. Create in-app notification (broadcast to all)
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = null, // Broadcast to all users
                Type = NotificationType.OpenNotificationCenter,
                Title = title,
                Message = message,
                Data = new { maintenanceStart, maintenanceEnd },
                Priority = NotificationPriority.High,
                ExpiresAt = maintenanceEnd
            }, cancellationToken);

            // 2. Broadcast push notification to all users
            await _pushNotificationService.BroadcastAsync(
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "system_maintenance" },
                    { "maintenanceStart", maintenanceStart.ToString("o") },
                    { "maintenanceEnd", maintenanceEnd.ToString("o") }
                },
                imageUrl: null,
                platform: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system maintenance notification");
        }
    }

    public async Task SendSecurityAlertNotificationAsync(
        Guid userId,
        string alertType,
        string description,
        string ipAddress,
        string location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var title = "⚠️ Cảnh báo bảo mật";
            var message = $"{alertType}: {description}. IP: {ipAddress}, Vị trí: {location}";

            // 1. Create in-app notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.OpenUserProfile,
                Title = title,
                Message = message,
                Data = new { alertType, description, ipAddress, location },
                Priority = NotificationPriority.Urgent
            }, cancellationToken);

            // 2. Send push notification
            await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                new Dictionary<string, string>
                {
                    { "type", "security_alert" },
                    { "alertType", alertType },
                    { "description", description },
                    { "ipAddress", ipAddress },
                    { "location", location },
                    { "actionType", "OpenUserProfile" }
                },
                imageUrl: null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security alert notification");
        }
    }

    #endregion
}