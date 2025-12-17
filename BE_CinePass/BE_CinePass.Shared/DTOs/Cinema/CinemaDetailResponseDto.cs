using BE_CinePass.Shared.DTOs.Movie;

namespace BE_CinePass.Shared.DTOs.Cinema;

public class CinemaDetailResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? BannerUrl { get; set; }
    public int TotalScreens { get; set; }
    public List<string>? Facilities { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Danh sách phim đang chiếu tại rạp này
    public List<MovieResponseDto> CurrentlyShowingMovies { get; set; } = new List<MovieResponseDto>();
}