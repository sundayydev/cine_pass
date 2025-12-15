namespace BE_CinePass.Shared.DTOs.MovieReview;

public class MovieReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid? MovieId { get; set; }
    public Guid? UserId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Optional: Include related data
    public string? UserName { get; set; }
    public string? MovieTitle { get; set; }
}
