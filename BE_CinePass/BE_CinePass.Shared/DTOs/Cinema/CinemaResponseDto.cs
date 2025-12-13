namespace BE_CinePass.Shared.DTOs.Cinema;

public class CinemaResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // Quan trọng cho SEO/Routing
    public string? Description { get; set; }
    
    // Địa điểm
    public string? Address { get; set; }
    public string? City { get; set; }
    
    // Tọa độ & Liên hệ
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    // Hình ảnh & Tiện ích
    public string? BannerUrl { get; set; }
    public int TotalScreens { get; set; }
    public List<string>? Facilities { get; set; }

    // Ngày tạo và cập nhật
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; }
}