namespace BE_CinePass.Shared.DTOs.Notification;

public class NotificationStatsDto
{
    public int TotalNotifications { get; set; }
    public int UnreadCount { get; set; }
    public int ReadCount { get; set; }
    public int TodayCount { get; set; }
    public int ThisWeekCount { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
    public Dictionary<string, int> ByPriority { get; set; } = new();
}