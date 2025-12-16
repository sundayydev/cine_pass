using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs;

/// <summary>
/// Response DTO for image upload
/// </summary>
public class UploadImageResponseDto
{
    /// <summary>
    /// Public ID from Cloudinary
    /// </summary>
    public string PublicId { get; set; } = string.Empty;

    /// <summary>
    /// URL of uploaded image
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Secure URL (HTTPS) of uploaded image
    /// </summary>
    public string SecureUrl { get; set; } = string.Empty;

    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Image format (jpg, png, etc.)
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Bytes { get; set; }
}

/// <summary>
/// Response DTO for multiple images upload
/// </summary>
public class UploadMultipleImagesResponseDto
{
    /// <summary>
    /// List of uploaded images
    /// </summary>
    public List<UploadImageResponseDto> Images { get; set; } = new();

    /// <summary>
    /// Number of successfully uploaded images
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed uploads
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Error messages if any
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
