namespace BE_CinePass.Core.Configurations;

/// <summary>
/// Cloudinary configuration settings
/// </summary>
public class CloudinarySettings
{
    /// <summary>
    /// Cloud name
    /// </summary>
    public string CloudName { get; set; } = string.Empty;

    /// <summary>
    /// API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API Secret
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Folder to store uploaded images (optional)
    /// </summary>
    public string Folder { get; set; } = "cinepass";
}
