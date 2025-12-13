using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class ScreenRepository : BaseRepository<Screen>, IScreenRepository
{
    public ScreenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Screen>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.CinemaId == cinemaId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Screen?> GetWithSeatsAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Seats)
                .ThenInclude(seat => seat.SeatType)
            .FirstOrDefaultAsync(s => s.Id == screenId, cancellationToken);
    }
}

