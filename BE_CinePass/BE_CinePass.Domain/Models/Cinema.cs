using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_CinePass.Domain.Models;

[Table("cinemas")]
public class Cinema
{
    // --- Định danh ---
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    // --- Thông tin cơ bản ---
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    // --- Liên hệ & Địa điểm ---
    [MaxLength(500)]
    [Column("address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Column("city")]
    public string? City { get; set; }

    [MaxLength(50)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(500)]
    [Column("website")]
    public string? Website { get; set; }

    // --- Tọa độ ---
    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    // --- Hình ảnh ---
    [Column("banner_url")]
    public string? BannerUrl { get; set; }

    // --- Thông tin phụ ---
    [Column("total_screens")]
    public int TotalScreens { get; set; } = 0;

    // Lưu ý: Để map được mảng text[] của Postgres, bạn cần sử dụng Npgsql
    [Column("facilities", TypeName = "text[]")]
    public List<string>? Facilities { get; set; }

    // --- Trạng thái & Audit ---
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation properties ---
    public virtual ICollection<Screen> Screens { get; set; } = new List<Screen>();
}