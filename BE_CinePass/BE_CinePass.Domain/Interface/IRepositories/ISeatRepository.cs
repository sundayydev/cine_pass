using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho Seat với các phương thức đặc thù
/// </summary>
public interface ISeatRepository : IRepository<Seat>
{
    Task<IEnumerable<Seat>> GetByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Seat>> GetActiveSeatsByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default);
    Task<Seat?> GetBySeatCodeAsync(Guid screenId, string seatCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Seat>> GetBySeatTypeAsync(string seatTypeCode, CancellationToken cancellationToken = default);
    Task<bool> IsSeatAvailableAsync(Guid seatId, Guid showtimeId, CancellationToken cancellationToken = default);
}

