using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.User;
using BE_CinePass.Shared.DTOs.Auth;
using BE_CinePass.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthTokenService _authTokenService;
    private readonly MemberPointService _memberPointService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserService userService, AuthTokenService authTokenService, MemberPointService memberPointService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _authTokenService = authTokenService;
        _memberPointService = memberPointService;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] UserCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var user = await _userService.CreateAsync(dto, cancellationToken);
            var tokens = await _authTokenService.GenerateTokensAsync(user, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponseDto<AuthResponseDto>.SuccessResult(tokens, "Đăng ký thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ApiResponseDto<AuthResponseDto>.ErrorResult("Lỗi khi đăng ký"));
        }
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] UserLoginDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ"));

            var isValid = await _userService.ValidateCredentialsAsync(dto.Email, dto.Password, cancellationToken);
            if (!isValid)
                return Unauthorized(ApiResponseDto<AuthResponseDto>.ErrorResult("Email hoặc mật khẩu không đúng"));

            var user = await _userService.GetByEmailAsync(dto.Email, cancellationToken);
            var tokens = await _authTokenService.GenerateTokensAsync(user!, cancellationToken);
            return Ok(ApiResponseDto<AuthResponseDto>.SuccessResult(tokens, "Đăng nhập thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponseDto<AuthResponseDto>.ErrorResult("Lỗi khi đăng nhập"));
        }
    }

    /// <summary>
    /// Làm mới access token từ refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResult("Dữ liệu không hợp lệ"));

        var tokens = await _authTokenService.RefreshTokensAsync(dto.RefreshToken, cancellationToken);
        if (tokens is null)
            return Unauthorized(ApiResponseDto<AuthResponseDto>.ErrorResult("Refresh token không hợp lệ hoặc đã hết hạn"));

        return Ok(ApiResponseDto<AuthResponseDto>.SuccessResult(tokens, "Làm mới token thành công"));
    }

    /// <summary>
    /// Đăng xuất và thu hồi refresh token
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<bool>>> Logout([FromBody] RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponseDto<bool>.ErrorResult("Dữ liệu không hợp lệ"));

        await _authTokenService.RevokeRefreshTokenAsync(dto.RefreshToken, cancellationToken);
        return Ok(ApiResponseDto<bool>.SuccessResult(true, "Đăng xuất thành công"));
    }

    /// <summary>
    /// Lấy thông tin người dùng hiện tại (Profile, Points)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<AuthMeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<AuthMeResponseDto>>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        try
        {
            // Lấy user ID từ JWT claims sử dụng extension method
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<AuthMeResponseDto>.ErrorResult("Token không hợp lệ"));

            // Lấy thông tin user
            var userProfile = await _userService.GetByIdAsync(userId.Value, cancellationToken);
            if (userProfile == null)
                return NotFound(ApiResponseDto<AuthMeResponseDto>.ErrorResult("Không tìm thấy người dùng"));

            // Lấy thông tin điểm thành viên
            var memberPoints = await _memberPointService.GetByUserIdAsync(userId.Value, cancellationToken);

            var response = new AuthMeResponseDto
            {
                Profile = userProfile,
                Points = memberPoints
            };

            return Ok(ApiResponseDto<AuthMeResponseDto>.SuccessResult(response, "Lấy thông tin người dùng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user info");
            return StatusCode(500, ApiResponseDto<AuthMeResponseDto>.ErrorResult("Lỗi khi lấy thông tin người dùng"));
        }
    }

    /// <summary>
    /// Helper method để CreatedAtAction có thể reference
    /// </summary>
    [HttpGet("user/{id}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound(ApiResponseDto<UserResponseDto>.ErrorResult($"Không tìm thấy người dùng có ID {id}"));

        return Ok(ApiResponseDto<UserResponseDto>.SuccessResult(user));
    }
}

