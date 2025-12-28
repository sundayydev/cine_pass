namespace BE_CinePass.Shared.DTOs.Notification
{
    public class NotificationSettingsResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool EnableUpcomingShowtime { get; set; }
        public bool EnableOrderStatus { get; set; }
        public bool EnablePromotion { get; set; }
        public bool EnableSystem { get; set; }
        public int ShowtimeReminderMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
