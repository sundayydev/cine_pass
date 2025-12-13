using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class OrderTicketRepository : BaseRepository<OrderTicket>, IOrderTicketRepository
{
    public OrderTicketRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderTicket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ot => ot.OrderId == orderId)
            .Include(ot => ot.Seat)
            .Include(ot => ot.Showtime)
                .ThenInclude(st => st.Movie)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderTicket>> GetByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ot => ot.ShowtimeId == showtimeId)
            .Include(ot => ot.Seat)
            .Include(ot => ot.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderTicket>> GetBySeatIdAsync(Guid seatId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ot => ot.SeatId == seatId)
            .Include(ot => ot.Showtime)
            .Include(ot => ot.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSeatBookedAsync(Guid showtimeId, Guid seatId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(ot => ot.ShowtimeId == showtimeId && 
                           ot.SeatId == seatId &&
                           ot.Order.Status == OrderStatus.Confirmed, cancellationToken);
    }

    public async Task<IEnumerable<Guid>> GetBookedSeatIdsAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ot => ot.ShowtimeId == showtimeId &&
                        ot.Order.Status == OrderStatus.Confirmed)
            .Select(ot => ot.SeatId)
            .ToListAsync(cancellationToken);
    }
}

