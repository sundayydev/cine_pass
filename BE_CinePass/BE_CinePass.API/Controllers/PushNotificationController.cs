using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Services;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.DeviceToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/push")]
[Authorize(Roles = "Admin,Staff")]
public class PushNotificationController : ControllerBase
{
    private readonly FirebaseMessagingService _fcmService;
    private readonly DeviceTokenRepository _deviceTokenRepository;
    private readonly ILogger<PushNotificationController> _logger;

    public PushNotificationController(
        FirebaseMessagingService fcmService,
        DeviceTokenRepository deviceTokenRepository,
        ILogger<PushNotificationController> logger)
    {
        _fcmService = fcmService;
        _deviceTokenRepository = deviceTokenRepository;
        _logger = logger;
    }

    /// <summary>
    /// Send test push notification to a specific user
    /// </summary>
    [HttpPost("send-test")]
    [ProducesResponseType(typeof(ApiResponseDto<PushNotificationResultDto>), 200)]
    public async Task<IActionResult> SendTestPush([FromBody] SendTestPushDto dto)
    {
        try
        {
            // Get user's active device tokens
            var deviceTokens = await _deviceTokenRepository.GetActiveByUserIdAsync(dto.UserId);

            if (!deviceTokens.Any())
            {
                return Ok(new ApiResponseDto<PushNotificationResultDto>
                {
                    Success = false,
                    Message = "User has no registered device tokens",
                    Data = new PushNotificationResultDto
                    {
                        Success = false,
                        TotalAttempted = 0,
                        Message = "No device tokens found"
                    }
                });
            }

            var tokens = deviceTokens.Select(dt => dt.Token).ToList();
            var (successCount, failureCount, invalidTokens) = 
                await _fcmService.SendToDevicesAsync(
                    tokens,
                    dto.Title,
                    dto.Body,
                    dto.Data,
                    dto.ImageUrl);

            _logger.LogInformation(
                "Test push sent to user {UserId}. Success: {Success}, Failed: {Failed}",
                dto.UserId, successCount, failureCount);

            return Ok(new ApiResponseDto<PushNotificationResultDto>
            {
                Success = successCount > 0,
                Message = $"Push notification sent to {successCount} device(s)",
                Data = new PushNotificationResultDto
                {
                    Success = successCount > 0,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    TotalAttempted = tokens.Count,
                    InvalidTokens = invalidTokens,
                    Message = $"Sent to {successCount}/{tokens.Count} devices"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test push notification");
            return BadRequest(new ApiResponseDto<PushNotificationResultDto>
            {
                Success = false,
                Message = "Failed to send push notification",
                Errors = new List<string> { ex.Message },
                Data = new PushNotificationResultDto
                {
                    Success = false,
                    Error = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Broadcast push notification to all users
    /// </summary>
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponseDto<PushNotificationResultDto>), 200)]
    public async Task<IActionResult> BroadcastPush([FromBody] BroadcastPushDto dto)
    {
        try
        {
            // Get all active device tokens
            var deviceTokens = string.IsNullOrEmpty(dto.Platform)
                ? await _deviceTokenRepository.GetAllActiveAsync()
                : await _deviceTokenRepository.GetActiveByPlatformAsync(dto.Platform);

            if (!deviceTokens.Any())
            {
                return Ok(new ApiResponseDto<PushNotificationResultDto>
                {
                    Success = false,
                    Message = "No active device tokens found",
                    Data = new PushNotificationResultDto
                    {
                        Success = false,
                        TotalAttempted = 0,
                        Message = "No devices to send to"
                    }
                });
            }

            var tokens = deviceTokens.Select(dt => dt.Token).ToList();
            var (successCount, failureCount, invalidTokens) = 
                await _fcmService.SendToDevicesAsync(
                    tokens,
                    dto.Title,
                    dto.Body,
                    dto.Data,
                    dto.ImageUrl);

            _logger.LogInformation(
                "Broadcast push sent to {Total} devices. Success: {Success}, Failed: {Failed}",
                tokens.Count, successCount, failureCount);

            return Ok(new ApiResponseDto<PushNotificationResultDto>
            {
                Success = successCount > 0,
                Message = $"Broadcast sent to {successCount} device(s)",
                Data = new PushNotificationResultDto
                {
                    Success = successCount > 0,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    TotalAttempted = tokens.Count,
                    InvalidTokens = invalidTokens,
                    Message = $"Broadcast: {successCount}/{tokens.Count} successful"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting push notification");
            return BadRequest(new ApiResponseDto<PushNotificationResultDto>
            {
                Success = false,
                Message = "Failed to broadcast push notification",
                Errors = new List<string> { ex.Message },
                Data = new PushNotificationResultDto
                {
                    Success = false,
                    Error = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Send push notification to specific platform
    /// </summary>
    [HttpPost("send-by-platform")]
    [ProducesResponseType(typeof(ApiResponseDto<PushNotificationResultDto>), 200)]
    public async Task<IActionResult> SendByPlatform(
        [FromQuery] string platform,
        [FromBody] BroadcastPushDto dto)
    {
        try
        {
            if (!new[] { "ios", "android", "web" }.Contains(platform.ToLower()))
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Invalid platform. Must be ios, android, or web"
                });
            }

            var deviceTokens = await _deviceTokenRepository.GetActiveByPlatformAsync(platform.ToLower());

            if (!deviceTokens.Any())
            {
                return Ok(new ApiResponseDto<PushNotificationResultDto>
                {
                    Success = false,
                    Message = $"No active {platform} device tokens found",
                    Data = new PushNotificationResultDto
                    {
                        Success = false,
                        TotalAttempted = 0
                    }
                });
            }

            var tokens = deviceTokens.Select(dt => dt.Token).ToList();
            var (successCount, failureCount, invalidTokens) = 
                await _fcmService.SendToDevicesAsync(
                    tokens,
                    dto.Title,
                    dto.Body,
                    dto.Data,
                    dto.ImageUrl);

            _logger.LogInformation(
                "Push sent to {Platform}. Total: {Total}, Success: {Success}, Failed: {Failed}",
                platform, tokens.Count, successCount, failureCount);

            return Ok(new ApiResponseDto<PushNotificationResultDto>
            {
                Success = successCount > 0,
                Message = $"Sent to {successCount} {platform} device(s)",
                Data = new PushNotificationResultDto
                {
                    Success = successCount > 0,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    TotalAttempted = tokens.Count,
                    InvalidTokens = invalidTokens
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push to platform {Platform}", platform);
            return BadRequest(new ApiResponseDto<PushNotificationResultDto>
            {
                Success = false,
                Message = "Failed to send push notification",
                Errors = new List<string> { ex.Message },
                Data = new PushNotificationResultDto
                {
                    Success = false,
                    Error = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Check Firebase connection status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    public IActionResult GetStatus()
    {
        var isInitialized = _fcmService.IsInitialized();

        return Ok(new ApiResponseDto<object>
        {
            Success = isInitialized,
            Message = isInitialized 
                ? "Firebase Cloud Messaging is initialized and ready"
                : "Firebase Cloud Messaging is not initialized. Check configuration.",
            Data = new
            {
                IsInitialized = isInitialized,
                Service = "Firebase Cloud Messaging"
            }
        });
    }
}