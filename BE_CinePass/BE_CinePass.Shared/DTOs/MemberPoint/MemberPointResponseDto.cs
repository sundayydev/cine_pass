namespace BE_CinePass.Shared.DTOs.MemberPoint;

public class MemberPointResponseDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Optional: Include user info
    public string? UserEmail { get; set; }
}
