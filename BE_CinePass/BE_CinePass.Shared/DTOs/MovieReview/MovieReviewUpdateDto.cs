using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.MovieReview;

public class MovieReviewUpdateDto
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}
