using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Cinema;

public class CinemaCreateDto
{
    // --- Thông tin cơ bản ---
    [Required(ErrorMessage = "Tên rạp không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên rạp tối đa 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug chỉ được chứa chữ thường, số và dấu gạch ngang")]
    public string? Slug { get; set; }
    // Note: Nếu null, Backend nên tự generate từ Name (VD: "CGV Ba Triệu" -> "cgv-ba-trieu")

    public string? Description { get; set; }

    // --- Liên hệ & Địa điểm ---
    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(50)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Url(ErrorMessage = "Website phải là một đường dẫn hợp lệ")]
    [MaxLength(500)]
    public string? Website { get; set; }

    // --- Tọa độ (Validation Range cho vĩ độ/kinh độ) ---
    [Range(-90, 90, ErrorMessage = "Vĩ độ (Latitude) phải từ -90 đến 90")]
    public double? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Kinh độ (Longitude) phải từ -180 đến 180")]
    public double? Longitude { get; set; }

    // --- Hình ảnh ---
    [Url(ErrorMessage = "Banner URL không hợp lệ")]
    public string? BannerUrl { get; set; }

    // --- Thông tin phụ ---
    [Range(0, 100, ErrorMessage = "Số lượng phòng chiếu không hợp lệ")]
    public int TotalScreens { get; set; } = 0;

    public List<string>? Facilities { get; set; } = new List<string>();

    // --- Trạng thái ---
    public bool IsActive { get; set; } = true;
}