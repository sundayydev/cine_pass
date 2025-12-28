namespace BE_CinePass.Shared.DTOs.Notification
{
    public class NotificationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; } // Parsed JSON
        public bool IsRead { get; set; }
        public string? ImageUrl { get; set; }
        public string? ActionType { get; set; }
        public string? ActionData { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
