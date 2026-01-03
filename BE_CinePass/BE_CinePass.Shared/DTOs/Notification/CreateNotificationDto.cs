

using BE_CinePass.Shared.Common;

namespace BE_CinePass.Shared.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public Guid? UserId { get; set; } // NULL = broadcast to all?
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string? ImageUrl { get; set; }
        public NotificationActionType? ActionType { get; set; }
        public string? ActionData { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public DateTime? ExpiresAt { get; set; }
    }
}
