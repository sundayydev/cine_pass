using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho Showtime với các phương thức đặc thù
/// </summary>
public interface IShowtimeRepository : IRepository<Showtime>
{
    Task<IEnumerable<Showtime>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetActiveShowtimesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetByMovieAndDateAsync(Guid movieId, DateTime date, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingShowtimeAsync(Guid screenId, DateTime startTime, DateTime endTime, Guid? excludeShowtimeId = null, CancellationToken cancellationToken = default);
}

