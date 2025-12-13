using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class ShowtimeRepository : BaseRepository<Showtime>, IShowtimeRepository
{
    public ShowtimeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Showtime>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.MovieId == movieId && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Showtime>> GetByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ScreenId == screenId && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Showtime>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);
        
        return await _dbSet
            .Where(s => s.StartTime >= startOfDay && s.StartTime < endOfDay && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Showtime>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.StartTime >= startDate && s.StartTime <= endDate && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Showtime>> GetActiveShowtimesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Showtime>> GetByMovieAndDateAsync(Guid movieId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);
        
        return await _dbSet
            .Where(s => s.MovieId == movieId && 
                       s.StartTime >= startOfDay && 
                       s.StartTime < endOfDay && 
                       s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingShowtimeAsync(Guid screenId, DateTime startTime, DateTime endTime, Guid? excludeShowtimeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(s => s.ScreenId == screenId &&
                       s.IsActive &&
                       ((s.StartTime < endTime && s.EndTime > startTime)));

        if (excludeShowtimeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeShowtimeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}

