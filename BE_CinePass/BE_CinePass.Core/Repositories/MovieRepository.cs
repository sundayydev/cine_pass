using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class MovieRepository : BaseRepository<Movie>, IMovieRepository
{
    public MovieRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Include(m => m.MovieReviews)
                .ThenInclude(mr => mr.User)
            .FirstOrDefaultAsync(m => m.Slug == slug, cancellationToken);
    }

    public new async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Include(m => m.MovieReviews)
                .ThenInclude(mr => mr.User)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetByStatusAsync(MovieStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetNowShowingAsync(CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(MovieStatus.Showing, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetComingSoonAsync(CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(MovieStatus.ComingSoon, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(m => m.Title.ToLower().Contains(lowerSearchTerm) ||
                       (m.Description != null && m.Description.ToLower().Contains(lowerSearchTerm)))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(m => m.Slug == slug, cancellationToken);
    }
}

