using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

/// <summary>
/// Controller for image upload operations using Cloudinary
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UploadController : ControllerBase
{
    private readonly CloudinaryService _cloudinaryService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        CloudinaryService cloudinaryService,
        ILogger<UploadController> logger)
    {
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single image
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="folder">Optional folder name (default: cinepass)</param>
    /// <returns>Uploaded image details including URL</returns>
    /// <response code="200">Image uploaded successfully</response>
    /// <response code="400">Invalid file or validation error</response>
    /// <response code="401">Unauthorized - Login required</response>
    /// <response code="500">Server error during upload</response>
    [HttpPost("image")]
    [Authorize(Roles = "Admin,Staff")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadImageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromQuery] string? folder = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            _logger.LogInformation("Uploading image: {FileName}, Size: {Size} bytes",
                file.FileName, file.Length);

            var result = await _cloudinaryService.UploadImageAsync(file, folder);

            if (result == null)
            {
                return StatusCode(500, "Failed to upload image");
            }

            _logger.LogInformation("Image uploaded successfully: {PublicId}", result.PublicId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }

    /// <summary>
    /// Upload multiple images at once
    /// </summary>
    /// <param name="files">Array of image files to upload</param>
    /// <param name="folder">Optional folder name (default: cinepass)</param>
    /// <returns>List of uploaded images with success/failure count</returns>
    /// <response code="200">Images processed (check SuccessCount and FailedCount)</response>
    /// <response code="400">Invalid request or no files provided</response>
    /// <response code="401">Unauthorized - Login required</response>
    [HttpPost("images")]
    [Authorize(Roles = "Admin,Staff")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadMultipleImagesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadMultipleImages(
        [FromForm] List<IFormFile> files,
        [FromQuery] string? folder = null)
    {
        try
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files provided");
            }

            _logger.LogInformation("Uploading {Count} images", files.Count);

            var result = await _cloudinaryService.UploadMultipleImagesAsync(files, folder);

            _logger.LogInformation("Upload completed: {Success} successful, {Failed} failed",
                result.SuccessCount, result.FailedCount);

            var message = result.FailedCount > 0
                ? $"Upload completed with {result.FailedCount} failures"
                : "All images uploaded successfully";

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple images");
            return StatusCode(500, "An error occurred while uploading images");
        }
    }

    /// <summary>
    /// Delete an image from Cloudinary
    /// </summary>
    /// <param name="publicId">Public ID of the image to delete</param>
    /// <returns>Deletion status</returns>
    /// <response code="200">Image deleted successfully</response>
    /// <response code="400">Invalid public ID</response>
    /// <response code="401">Unauthorized - Login required</response>
    /// <response code="404">Image not found</response>
    [HttpDelete("image/{publicId}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(string publicId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return BadRequest("Public ID is required");
            }

            // Decode publicId if it contains URL-encoded characters (like folder/filename)
            publicId = Uri.UnescapeDataString(publicId);

            _logger.LogInformation("Deleting image: {PublicId}", publicId);

            var result = await _cloudinaryService.DeleteImageAsync(publicId);

            if (!result)
            {
                return NotFound("Image not found or already deleted");
            }

            _logger.LogInformation("Image deleted successfully: {PublicId}", publicId);

            return Ok(true);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image");
            return StatusCode(500, "An error occurred while deleting the image");
        }
    }

    /// <summary>
    /// Get transformed image URL
    /// </summary>
    /// <param name="publicId">Public ID of the image</param>
    /// <param name="width">Target width in pixels</param>
    /// <param name="height">Target height in pixels</param>
    /// <param name="crop">Crop mode: fill, fit, scale, crop, thumb, pad, limit, mfit, lfill</param>
    /// <returns>Transformed image URL</returns>
    /// <response code="200">Returns transformed image URL</response>
    /// <response code="400">Invalid parameters</response>
    [HttpGet("transform/{publicId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetTransformedImageUrl(
        string publicId,
        [FromQuery] int? width = null,
        [FromQuery] int? height = null,
        [FromQuery] string crop = "fill")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return BadRequest("Public ID is required");
            }

            // Decode publicId if it contains URL-encoded characters
            publicId = Uri.UnescapeDataString(publicId);

            var url = _cloudinaryService.GetTransformedImageUrl(publicId, width, height, crop);

            return Ok(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transformed URL");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Upload image for a specific entity type
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="entityType">Entity type: movie, actor, cinema, product, banner</param>
    /// <param name="entityId">Optional entity ID to organize images</param>
    /// <returns>Uploaded image details</returns>
    /// <response code="200">Image uploaded successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - Login required</response>
    [HttpPost("entity/{entityType}")]
    [Authorize(Roles = "Admin,Staff")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadImageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadEntityImage(
        IFormFile file,
        string entityType,
        [FromQuery] string? entityId = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            // Validate entity type
            var validEntityTypes = new[] { "movie", "actor", "cinema", "product", "banner", "user" };
            if (!validEntityTypes.Contains(entityType.ToLower()))
            {
                return BadRequest($"Invalid entity type. Valid types: {string.Join(", ", validEntityTypes)}");
            }

            // Create folder structure: cinepass/{entityType}/{entityId}
            var folder = string.IsNullOrWhiteSpace(entityId)
                ? $"cinepass/{entityType.ToLower()}"
                : $"cinepass/{entityType.ToLower()}/{entityId}";

            _logger.LogInformation("Uploading {EntityType} image to folder: {Folder}",
                entityType, folder);

            var result = await _cloudinaryService.UploadImageAsync(file, folder);

            if (result == null)
            {
                return StatusCode(500, "Failed to upload image");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading entity image");
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }
}
