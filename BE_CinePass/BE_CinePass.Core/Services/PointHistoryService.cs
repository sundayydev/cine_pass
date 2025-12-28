using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.PointHistory;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class PointHistoryService
{
    private readonly ApplicationDbContext _context;

    public PointHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PointHistoryResponseDto>> GetByUserIdAsync(
        Guid userId,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PointHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt);

        if (limit.HasValue)
            query = (IOrderedQueryable<PointHistory>)query.Take(limit.Value);

        var histories = await query.ToListAsync(cancellationToken);

        return histories.Select(MapToResponseDto).ToList();
    }

    public async Task<PointHistory> CreateAsync(
        PointHistory pointHistory,
        CancellationToken cancellationToken = default)
    {
        _context.PointHistories.Add(pointHistory);
        await _context.SaveChangesAsync(cancellationToken);
        return pointHistory;
    }

    private static PointHistoryResponseDto MapToResponseDto(PointHistory pointHistory)
    {
        return new PointHistoryResponseDto
        {
            Id = pointHistory.Id,
            UserId = pointHistory.UserId,
            Points = pointHistory.Points,
            Type = pointHistory.Type?.ToString() ?? "",
            Description = pointHistory.Description,
            OrderId = pointHistory.OrderId,
            VoucherId = pointHistory.VoucherId,
            ExpiresAt = pointHistory.ExpiresAt,
            CreatedAt = pointHistory.CreatedAt
        };
    }
}
