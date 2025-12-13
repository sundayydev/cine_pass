using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class CinemaRepository : BaseRepository<Cinema>, ICinemaRepository
{
    public CinemaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Cinema>> GetActiveCinemasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cinema>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && c.City != null && c.City.ToLower() == city.ToLower())
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Cinema?> GetWithScreensAsync(Guid cinemaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Screens)
            .FirstOrDefaultAsync(c => c.Id == cinemaId, cancellationToken);
    }
}

