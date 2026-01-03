namespace BE_CinePass.Shared.DTOs.DeviceToken;

public class PlatformStats
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public double ActivePercentage => Total > 0 ? (double)Active / Total * 100 : 0;
}