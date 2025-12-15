using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class MemberPointRepository
{
    private readonly ApplicationDbContext _context;

    public MemberPointRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MemberPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MemberPoints
            .Include(mp => mp.User)
            .FirstOrDefaultAsync(mp => mp.Id == id, cancellationToken);
    }

    public async Task<MemberPoint?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.MemberPoints
            .Include(mp => mp.User)
            .FirstOrDefaultAsync(mp => mp.UserId == userId, cancellationToken);
    }

    public async Task<List<MemberPoint>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MemberPoints
            .Include(mp => mp.User)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MemberPoint memberPoint, CancellationToken cancellationToken = default)
    {
        await _context.MemberPoints.AddAsync(memberPoint, cancellationToken);
    }

    public void Update(MemberPoint memberPoint)
    {
        _context.MemberPoints.Update(memberPoint);
    }

    public async Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var memberPoint = await _context.MemberPoints.FindAsync(new object[] { id }, cancellationToken);
        if (memberPoint == null)
            return false;

        _context.MemberPoints.Remove(memberPoint);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.MemberPoints.AnyAsync(mp => mp.UserId == userId, cancellationToken);
    }
}
