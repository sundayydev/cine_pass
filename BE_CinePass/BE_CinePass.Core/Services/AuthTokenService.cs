using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.User;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services;

public class AuthTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IDistributedCache _cache;
    private readonly UserService _userService;

    public AuthTokenService(
        IOptions<JwtSettings> jwtOptions,
        IDistributedCache cache,
        UserService userService,
        ILogger<AuthTokenService> logger)
    {
        _jwtSettings = jwtOptions.Value;
        _cache = cache;
        _userService = userService;
    }

    public async Task<AuthResponseDto> GenerateTokensAsync(UserResponseDto user, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var accessExpiresAt = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var refreshExpiresAt = now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        // Create claims with role as string name (not enum value)
        var roleName = user.Role.ToString(); // "Customer", "Staff", or "Admin"

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, roleName), // Standard role claim for [Authorize(Roles = "...")]
            new("role", roleName), // Custom role claim for easier access
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add name claim if available
        if (!string.IsNullOrEmpty(user.FullName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));
        }

        var accessToken = BuildJwtToken(claims, accessExpiresAt);
        var refreshToken = GenerateRefreshToken();

        await _cache.SetStringAsync(
            BuildRefreshCacheKey(refreshToken),
            user.Id.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays)
            },
            cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task<AuthResponseDto?> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildRefreshCacheKey(refreshToken);
        var cachedUserId = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (string.IsNullOrEmpty(cachedUserId) || !Guid.TryParse(cachedUserId, out var userId))
        {
            return null;
        }

        var user = await _userService.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            return null;
        }

        // Rotate refresh token: remove old and issue new
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        return await GenerateTokensAsync(user, cancellationToken);
    }

    public Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return _cache.RemoveAsync(BuildRefreshCacheKey(refreshToken), cancellationToken);
    }

    private string BuildJwtToken(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private static string BuildRefreshCacheKey(string refreshToken) => $"refresh:{refreshToken}";
}

