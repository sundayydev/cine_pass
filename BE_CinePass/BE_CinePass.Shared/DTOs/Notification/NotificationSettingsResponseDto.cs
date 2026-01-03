namespace BE_CinePass.Shared.DTOs.Notification
{
    public class NotificationSettingsResponseDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; } 
        public bool EnableUpcomingShowtime { get; set; }
        public bool EnableOrderStatus { get; set; }
        public bool EnablePromotion { get; set; }
        public bool EnableSystem { get; set; }
        public int ShowtimeReminderMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
