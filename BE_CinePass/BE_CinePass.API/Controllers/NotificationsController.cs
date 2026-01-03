using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Notification;
using BE_CinePass.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly NotificationSettingsService _settingsService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        NotificationService notificationService,
        NotificationSettingsService settingsService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách thông báo của user hiện tại
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificationResponseDto>>> GetMyNotifications(
        [FromQuery] int limit = 50,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<List<NotificationResponseDto>>.ErrorResult("Unauthorized"));

            var notifications = await _notificationService.GetUserNotificationsAsync(
                userId.Value, limit, unreadOnly, cancellationToken);

            return Ok(ApiResponseDto<List<NotificationResponseDto>>.SuccessResult(
                notifications,
                $"Lấy {notifications.Count} thông báo thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user notifications");
            return StatusCode(500, ApiResponseDto<List<NotificationResponseDto>>.ErrorResult("Lỗi khi lấy thông báo"));
        }
    }

    /// <summary>
    /// Lấy số lượng thông báo chưa đọc
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<int>.ErrorResult("Unauthorized"));

            var count = await _notificationService.GetUnreadCountAsync(userId.Value, cancellationToken);
            return Ok(ApiResponseDto<int>.SuccessResult(count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return StatusCode(500, ApiResponseDto<int>.ErrorResult("Lỗi khi lấy số thông báo chưa đọc"));
        }
    }

    /// <summary>
    /// Đánh dấu thông báo đã đọc
    /// </summary>
    [HttpPut("{id}/mark-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<object>.ErrorResult("Unauthorized"));

            var result = await _notificationService.MarkAsReadAsync(id, userId.Value, cancellationToken);
            
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy thông báo"));

            return Ok(ApiResponseDto<object>.SuccessResult("Đã đánh dấu đã đọc"));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi đánh dấu đã đọc"));
        }
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo đã đọc
    /// </summary>
    [HttpPut("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<object>.ErrorResult("Unauthorized"));

            await _notificationService.MarkAllAsReadAsync(userId.Value, cancellationToken);
            return Ok(ApiResponseDto<object>.SuccessResult("Đã đánh dấu tất cả đã đọc"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi đánh dấu tất cả đã đọc"));
        }
    }

    /// <summary>
    /// Xóa thông báo
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<object>.ErrorResult("Unauthorized"));

            var result = await _notificationService.DeleteNotificationAsync(id, userId.Value, cancellationToken);
            
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy thông báo"));

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa thông báo"));
        }
    }

    /// <summary>
    /// Lấy cài đặt thông báo của user
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(NotificationSettingsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationSettingsResponseDto>> GetSettings(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<NotificationSettingsResponseDto>.ErrorResult("Unauthorized"));

            var settings = await _settingsService.GetOrCreateSettingsAsync(userId.Value, cancellationToken);
            return Ok(ApiResponseDto<NotificationSettingsResponseDto>.SuccessResult(settings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification settings");
            return StatusCode(500, ApiResponseDto<NotificationSettingsResponseDto>.ErrorResult("Lỗi khi lấy cài đặt"));
        }
    }

    /// <summary>
    /// Cập nhật cài đặt thông báo
    /// </summary>
    [HttpPut("settings")]
    [ProducesResponseType(typeof(NotificationSettingsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationSettingsResponseDto>> UpdateSettings(
        [FromBody] UpdateNotificationSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<NotificationSettingsResponseDto>.ErrorResult("Unauthorized"));

            var settings = await _settingsService.UpdateSettingsAsync(userId.Value, dto, cancellationToken);
            return Ok(ApiResponseDto<NotificationSettingsResponseDto>.SuccessResult(
                settings, "Cập nhật cài đặt thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification settings");
            return StatusCode(500, ApiResponseDto<NotificationSettingsResponseDto>.ErrorResult("Lỗi khi cập nhật cài đặt"));
        }
    }

    /// <summary>
    /// Tạo thông báo (Admin only - for testing)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<NotificationResponseDto>> CreateNotification(
        [FromBody] CreateNotificationDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _notificationService.CreateNotificationAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetMyNotifications),
                new { id = notification.Id },
                ApiResponseDto<NotificationResponseDto>.SuccessResult(notification, "Tạo thông báo thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, ApiResponseDto<NotificationResponseDto>.ErrorResult("Lỗi khi tạo thông báo"));
        }
    }
}