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
    var now = DateTime.UtcNow;
    
    // Ghế không available nếu:
    // 1. Thuộc order đã Confirmed
    // 2. Thuộc order đang Pending và chưa hết hạn
    return !await _context.OrderTickets
        .AnyAsync(ot => ot.SeatId == seatId &&
                       ot.ShowtimeId == showtimeId &&
                       (ot.Order.Status == OrderStatus.Confirmed ||
                        (ot.Order.Status == OrderStatus.Pending && ot.Order.ExpireAt > now)),
                       cancellationToken);
}    

    /// <summary>
    /// Tìm ghế theo mã QR ordering (dùng cho tính năng order đồ ăn từ ghế)
    /// </summary>
    public async Task<Seat?> GetByQrCodeAsync(string qrOrderingCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Screen)
                .ThenInclude(sc => sc.Cinema)
            .FirstOrDefaultAsync(s => s.QrOrderingCode == qrOrderingCode && s.IsActive, cancellationToken);
    }
}

