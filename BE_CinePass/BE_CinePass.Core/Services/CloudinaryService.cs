using BE_CinePass.Core.Configurations;
using BE_CinePass.Shared.DTOs;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BE_CinePass.Core.Services;

/// <summary>
/// Service for uploading images to Cloudinary
/// </summary>
public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;

    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {
        _settings = settings.Value;

        var account = new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    /// <summary>
    /// Upload a single image to Cloudinary
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="folder">Optional folder name (overrides default)</param>
    /// <param name="transformation">Optional transformation parameters</param>
    /// <returns>Upload image response with URL and metadata</returns>
    public async Task<UploadImageResponseDto?> UploadImageAsync(
        IFormFile file,
        string? folder = null,
        string? transformation = null)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Validate file size (max 10MB)
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB");
        }

        try
        {
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder ?? _settings.Folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            // Apply transformation if provided
            if (!string.IsNullOrEmpty(transformation))
            {
                uploadParams.Transformation = new Transformation().RawTransformation(transformation);
            }

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload error: {uploadResult.Error.Message}");
            }

            return new UploadImageResponseDto
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.Url.ToString(),
                SecureUrl = uploadResult.SecureUrl.ToString(),
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Format = uploadResult.Format,
                Bytes = uploadResult.Bytes
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error uploading image to Cloudinary: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Upload multiple images to Cloudinary
    /// </summary>
    /// <param name="files">List of image files to upload</param>
    /// <param name="folder">Optional folder name (overrides default)</param>
    /// <returns>Upload response with list of uploaded images and errors</returns>
    public async Task<UploadMultipleImagesResponseDto> UploadMultipleImagesAsync(
        IEnumerable<IFormFile> files,
        string? folder = null)
    {
        var result = new UploadMultipleImagesResponseDto();

        foreach (var file in files)
        {
            try
            {
                var uploadResult = await UploadImageAsync(file, folder);
                if (uploadResult != null)
                {
                    result.Images.Add(uploadResult);
                    result.SuccessCount++;
                }
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Error uploading {file.FileName}: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Delete an image from Cloudinary by public ID
    /// </summary>
    /// <param name="publicId">Public ID of the image to delete</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            throw new ArgumentException("Public ID cannot be empty", nameof(publicId));
        }

        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting image from Cloudinary: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get transformed image URL
    /// </summary>
    /// <param name="publicId">Public ID of the image</param>
    /// <param name="width">Target width</param>
    /// <param name="height">Target height</param>
    /// <param name="crop">Crop mode (fill, fit, scale, etc.)</param>
    /// <returns>Transformed image URL</returns>
    public string GetTransformedImageUrl(
        string publicId,
        int? width = null,
        int? height = null,
        string crop = "fill")
    {
        var transformation = new Transformation();

        if (width.HasValue)
            transformation = transformation.Width(width.Value);

        if (height.HasValue)
            transformation = transformation.Height(height.Value);

        if (!string.IsNullOrEmpty(crop))
            transformation = transformation.Crop(crop);

        transformation = transformation.Quality("auto").FetchFormat("auto");

        return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
    }
}
