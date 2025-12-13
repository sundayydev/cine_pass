using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho OrderTicket với các phương thức đặc thù
/// </summary>
public interface IOrderTicketRepository : IRepository<OrderTicket>
{
    Task<IEnumerable<OrderTicket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderTicket>> GetByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderTicket>> GetBySeatIdAsync(Guid seatId, CancellationToken cancellationToken = default);
    Task<bool> IsSeatBookedAsync(Guid showtimeId, Guid seatId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetBookedSeatIdsAsync(Guid showtimeId, CancellationToken cancellationToken = default);
}

