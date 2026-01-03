namespace BE_CinePass.Shared.DTOs.DeviceToken;

public class DeviceTokenStatsDto
{
    public Dictionary<string, PlatformStats> PlatformStats { get; set; } = new();
    public int TotalTokens { get; set; }
    public int ActiveTokens { get; set; }
    public int InactiveTokens { get; set; }
}