using BE_CinePass.Core.Services;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.User;
using BE_CinePass.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userService.GetAllAsync(cancellationToken);
            return Ok(ApiResponseDto<List<UserResponseDto>>.SuccessResult(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, ApiResponseDto<List<UserResponseDto>>.ErrorResult("Lỗi khi lấy danh sách người dùng"));
        }
    }

    /// <summary>
    /// Lấy thông tin người dùng theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null)
                return NotFound(ApiResponseDto<UserResponseDto>.ErrorResult($"Không tìm thấy người dùng có ID {id}"));

            return Ok(ApiResponseDto<UserResponseDto>.SuccessResult(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id {UserId}", id);
            return StatusCode(500, ApiResponseDto<UserResponseDto>.ErrorResult("Lỗi khi lấy thông tin người dùng"));
        }
    }

    /// <summary>
    /// Lấy thông tin người dùng theo email
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(email, cancellationToken);
            if (user == null)
                return NotFound(ApiResponseDto<UserResponseDto>.ErrorResult($"Không tìm thấy người dùng với email {email}"));

            return Ok(ApiResponseDto<UserResponseDto>.SuccessResult(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return StatusCode(500, ApiResponseDto<UserResponseDto>.ErrorResult("Lỗi khi lấy thông tin người dùng"));
        }
    }

    /// <summary>
    /// Lấy danh sách người dùng theo vai trò
    /// </summary>
    [HttpGet("role/{role}")]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponseDto>>> GetByRole(UserRole role, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userService.GetByRoleAsync(role, cancellationToken);
            return Ok(ApiResponseDto<List<UserResponseDto>>.SuccessResult(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role {Role}", role);
            return StatusCode(500, ApiResponseDto<List<UserResponseDto>>.ErrorResult("Lỗi khi lấy danh sách người dùng"));
        }
    }

    /// <summary>
    /// Tạo người dùng mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] UserCreateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<UserResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var user = await _userService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponseDto<UserResponseDto>.SuccessResult(user, "Tạo người dùng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<UserResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponseDto<UserResponseDto>.ErrorResult("Lỗi khi tạo người dùng"));
        }
    }

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponseDto>> Update(Guid id, [FromBody] UserUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDto<UserResponseDto>.ErrorResult("Dữ liệu không hợp lệ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var user = await _userService.UpdateAsync(id, dto, cancellationToken);
            if (user == null)
                return NotFound(ApiResponseDto<UserResponseDto>.ErrorResult($"Không tìm thấy người dùng có ID {id}"));

            return Ok(ApiResponseDto<UserResponseDto>.SuccessResult(user, "Cập nhật thông tin thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<UserResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, ApiResponseDto<UserResponseDto>.ErrorResult("Lỗi khi cập nhật người dùng"));
        }
    }

    /// <summary>
    /// Xóa người dùng
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Không tìm thấy người dùng có ID {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi xóa người dùng"));
        }
    }

}

