using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.DeviceToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using BE_CinePass.Shared.DTOs.Common;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/device-tokens")]
[Authorize]
public class DeviceTokenController : ControllerBase
{
    private readonly DeviceTokenRepository _deviceTokenRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceTokenController> _logger;

    public DeviceTokenController(
        DeviceTokenRepository deviceTokenRepository,
        ApplicationDbContext context,
        ILogger<DeviceTokenController> logger)
    {
        _deviceTokenRepository = deviceTokenRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Register a device token for push notifications
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<DeviceTokenResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    public async Task<IActionResult> RegisterToken([FromBody] DeviceTokenRegisterDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Check if token already exists for this user
            var existingToken = await _deviceTokenRepository.FindByTokenAndUserAsync(
                dto.Token, userId);

            if (existingToken != null)
            {
                // Update existing token
                existingToken.Platform = dto.Platform;
                existingToken.DeviceModel = dto.DeviceModel;
                existingToken.AppVersion = dto.AppVersion;
                existingToken.IsActive = true;
                existingToken.UpdatedAt = DateTime.UtcNow;

                _deviceTokenRepository.Update(existingToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Device token updated for user {UserId}, platform: {Platform}",
                    userId, dto.Platform);

                return Ok(new ApiResponseDto<DeviceTokenResponseDto>
                {
                    Success = true,
                    Message = "Device token updated successfully",
                    Data = MapToResponseDto(existingToken)
                });
            }

            // Create new token
            var deviceToken = new DeviceToken
            {
                UserId = userId,
                Token = dto.Token,
                Platform = dto.Platform,
                DeviceModel = dto.DeviceModel,
                AppVersion = dto.AppVersion,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _deviceTokenRepository.AddAsync(deviceToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Device token registered for user {UserId}, platform: {Platform}",
                userId, dto.Platform);

            return Ok(new ApiResponseDto<DeviceTokenResponseDto>
            {
                Success = true,
                Message = "Device token registered successfully",
                Data = MapToResponseDto(deviceToken)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to register device token",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get all device tokens for current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<DeviceTokenResponseDto>>), 200)]
    public async Task<IActionResult> GetMyTokens()
    {
        try
        {
            var userId = GetCurrentUserId();
            var tokens = await _deviceTokenRepository.GetActiveByUserIdAsync(userId);

            return Ok(new ApiResponseDto<List<DeviceTokenResponseDto>>
            {
                Success = true,
                Data = tokens.Select(MapToResponseDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device tokens");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to get device tokens",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Update device token information
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<DeviceTokenResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
    public async Task<IActionResult> UpdateToken(Guid id, [FromBody] DeviceTokenUpdateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var token = await _deviceTokenRepository.GetByIdAsync(id);

            if (token == null || token.UserId != userId)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Device token not found"
                });
            }

            if (dto.DeviceModel != null)
                token.DeviceModel = dto.DeviceModel;

            if (dto.AppVersion != null)
                token.AppVersion = dto.AppVersion;

            if (dto.IsActive.HasValue)
                token.IsActive = dto.IsActive.Value;

            token.UpdatedAt = DateTime.UtcNow;

            _deviceTokenRepository.Update(token);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<DeviceTokenResponseDto>
            {
                Success = true,
                Message = "Device token updated successfully",
                Data = MapToResponseDto(token)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device token");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to update device token",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Unregister a specific device token
    /// </summary>
    [HttpDelete("{token}")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    public async Task<IActionResult> UnregisterToken(string token)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _deviceTokenRepository.DeactivateByTokenAsync(token, userId);

            if (!result)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Device token not found"
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Device token unregistered for user {UserId}",
                userId);

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Device token unregistered successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device token");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to unregister device token",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Unregister all device tokens for current user (logout)
    /// </summary>
    [HttpDelete("all")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    public async Task<IActionResult> UnregisterAllTokens()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _deviceTokenRepository.DeactivateAllForUserAsync(userId);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "All device tokens unregistered for user {UserId}. Count: {Count}",
                userId, count);

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = $"{count} device token(s) unregistered successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering all device tokens");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to unregister device tokens",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get device token statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponseDto<DeviceTokenStatsDto>), 200)]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var platformStats = await _deviceTokenRepository.GetStatsByPlatformAsync();

            var stats = new DeviceTokenStatsDto
            {
                PlatformStats = platformStats.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new PlatformStats
                    {
                        Total = kvp.Value.Total,
                        Active = kvp.Value.Active,
                        Inactive = kvp.Value.Total - kvp.Value.Active
                    }),
                TotalTokens = platformStats.Values.Sum(v => v.Total),
                ActiveTokens = platformStats.Values.Sum(v => v.Active),
                InactiveTokens = platformStats.Values.Sum(v => v.Total - v.Active)
            };

            return Ok(new ApiResponseDto<DeviceTokenStatsDto>
            {
                Success = true,
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device token statistics");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to get statistics",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }

    private static DeviceTokenResponseDto MapToResponseDto(DeviceToken token)
    {
        return new DeviceTokenResponseDto
        {
            Id = token.Id,
            UserId = token.UserId,
            Token = token.Token,
            Platform = token.Platform,
            DeviceModel = token.DeviceModel,
            AppVersion = token.AppVersion,
            IsActive = token.IsActive,
            LastUsedAt = token.LastUsedAt,
            CreatedAt = token.CreatedAt,
            UpdatedAt = token.UpdatedAt
        };
    }
}