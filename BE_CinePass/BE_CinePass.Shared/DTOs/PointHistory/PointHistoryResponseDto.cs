namespace BE_CinePass.Shared.DTOs.PointHistory;

public class PointHistoryResponseDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public int? Points { get; set; }
    public string? Type { get; set; } // 'purchase', 'refund', 'reward'
    public DateTime CreatedAt { get; set; }

    // Optional: Include user info
    public string? UserEmail { get; set; }
}
