using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class SeatRepository : BaseRepository<Seat>, ISeatRepository
{
    public SeatRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Seat>> GetByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ScreenId == screenId)
            .OrderBy(s => s.SeatRow)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Seat>> GetActiveSeatsByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ScreenId == screenId && s.IsActive)
            .OrderBy(s => s.SeatRow)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Seat?> GetBySeatCodeAsync(Guid screenId, string seatCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.ScreenId == screenId && s.SeatCode == seatCode, cancellationToken);
    }

    public async Task<IEnumerable<Seat>> GetBySeatTypeAsync(string seatTypeCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.SeatTypeCode == seatTypeCode && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSeatAvailableAsync(Guid seatId, Guid showtimeId, CancellationToken cancellationToken = default)
    {
        return !await _context.OrderTickets
            .AnyAsync(ot => ot.SeatId == seatId && 
                           ot.ShowtimeId == showtimeId &&
                           ot.Order.Status == OrderStatus.Confirmed, cancellationToken);
    }
}

