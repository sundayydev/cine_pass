namespace BE_CinePass.Shared.DTOs.Notification
{
    public class UpdateNotificationSettingsDto
    {
        public bool? EnableUpcomingShowtime { get; set; }
        public bool? EnableOrderStatus { get; set; }
        public bool? EnablePromotion { get; set; }
        public bool? EnableSystem { get; set; }
        public int? ShowtimeReminderMinutes { get; set; }
    }
}
