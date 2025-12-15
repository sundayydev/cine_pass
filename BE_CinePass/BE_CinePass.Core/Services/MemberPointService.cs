using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.MemberPoint;

namespace BE_CinePass.Core.Services;

public class MemberPointService
{
    private readonly MemberPointRepository _memberPointRepository;
    private readonly ApplicationDbContext _context;

    public MemberPointService(MemberPointRepository memberPointRepository, ApplicationDbContext context)
    {
        _memberPointRepository = memberPointRepository;
        _context = context;
    }

    public async Task<MemberPointResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByIdAsync(id, cancellationToken);
        return memberPoint == null ? null : MapToResponseDto(memberPoint);
    }

    public async Task<MemberPointResponseDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        return memberPoint == null ? null : MapToResponseDto(memberPoint);
    }

    public async Task<List<MemberPointResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var memberPoints = await _memberPointRepository.GetAllAsync(cancellationToken);
        return memberPoints.Select(MapToResponseDto).ToList();
    }

    public async Task<MemberPointResponseDto> CreateAsync(Guid userId, int initialPoints = 0, CancellationToken cancellationToken = default)
    {
        // Check if user already has member points
        if (await _memberPointRepository.ExistsAsync(userId, cancellationToken))
            throw new InvalidOperationException($"User {userId} already has member points");

        var memberPoint = new MemberPoint
        {
            UserId = userId,
            Points = initialPoints,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _memberPointRepository.AddAsync(memberPoint, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(memberPoint);
    }

    public async Task<MemberPointResponseDto?> AddPointsAsync(Guid userId, int points, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null)
            return null;

        memberPoint.Points += points;
        memberPoint.UpdatedAt = DateTime.UtcNow;

        _memberPointRepository.Update(memberPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(memberPoint);
    }

    public async Task<MemberPointResponseDto?> SubtractPointsAsync(Guid userId, int points, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _memberPointRepository.GetByUserIdAsync(userId, cancellationToken);
        if (memberPoint == null)
            return null;

        if (memberPoint.Points < points)
            throw new InvalidOperationException("Insufficient points");

        memberPoint.Points -= points;
        memberPoint.UpdatedAt = DateTime.UtcNow;

        _memberPointRepository.Update(memberPoint);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(memberPoint);
    }

    private static MemberPointResponseDto MapToResponseDto(MemberPoint memberPoint)
    {
        return new MemberPointResponseDto
        {
            Id = memberPoint.Id,
            UserId = memberPoint.UserId,
            Points = memberPoint.Points,
            CreatedAt = memberPoint.CreatedAt,
            UpdatedAt = memberPoint.UpdatedAt,
            UserEmail = memberPoint.User?.Email
        };
    }
}
