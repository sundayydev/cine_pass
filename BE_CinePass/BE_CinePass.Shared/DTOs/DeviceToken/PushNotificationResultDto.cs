namespace BE_CinePass.Shared.DTOs.DeviceToken;

public class PushNotificationResultDto
{
    public bool Success { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int TotalAttempted { get; set; }
    public List<string> InvalidTokens { get; set; } = new();
    public string? Message { get; set; }
    public string? Error { get; set; }
}