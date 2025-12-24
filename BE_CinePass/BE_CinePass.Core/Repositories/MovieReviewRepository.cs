using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class MovieReviewRepository : BaseRepository<MovieReview>
{
    public MovieReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MovieReview>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.User)
            .Where(r => r.MovieId == movieId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUserReviewedAsync(Guid userId, Guid movieId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(r => r.UserId == userId && r.MovieId == movieId, cancellationToken);
    }
}
