using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.User;
using System.Security.Cryptography;
using System.Text;

namespace BE_CinePass.Core.Services;

public class UserService
{
    private readonly UserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public UserService(UserRepository userRepository, ApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user == null ? null : MapToResponseDto(user);
    }

    public async Task<UserResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        return user == null ? null : MapToResponseDto(user);
    }

    public async Task<List<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToResponseDto).ToList();
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException($"Email {dto.Email} đã tồn tại");

        // Check if phone already exists (if provided)
        if (!string.IsNullOrEmpty(dto.Phone) && await _userRepository.PhoneExistsAsync(dto.Phone, cancellationToken))
            throw new InvalidOperationException($"Phone {dto.Phone} đã tồn tại");

        var user = new User
        {
            Email = dto.Email.ToLower(),
            Phone = dto.Phone,
            FullName = dto.FullName,
            Role = (Domain.Common.UserRole)dto.Role,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto?> UpdateAsync(Guid id, UserUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return null;

        // Check phone if changed
        if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone != user.Phone)
        {
            if (await _userRepository.PhoneExistsAsync(dto.Phone, cancellationToken))
                throw new InvalidOperationException($"Phone {dto.Phone} đã tồn tại");
            user.Phone = dto.Phone;
        }

        if (!string.IsNullOrEmpty(dto.FullName))
            user.FullName = dto.FullName;

        if (!string.IsNullOrEmpty(dto.Password))
            user.PasswordHash = HashPassword(dto.Password);

        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<List<UserResponseDto>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByRoleAsync((Domain.Common.UserRole)role, cancellationToken);
        return users.Select(MapToResponseDto).ToList();
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user == null)
            return false;

        return VerifyPassword(password, user.PasswordHash);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Role = (Shared.Common.UserRole)user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

