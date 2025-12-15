using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class MovieActorRepository
{
    private readonly ApplicationDbContext _context;

    public MovieActorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MovieActor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MovieActors
            .Include(ma => ma.Movie)
            .Include(ma => ma.Actor)
            .FirstOrDefaultAsync(ma => ma.Id == id, cancellationToken);
    }

    public async Task<List<MovieActor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MovieActors
            .Include(ma => ma.Movie)
            .Include(ma => ma.Actor)
            .OrderByDescending(ma => ma.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MovieActor>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return await _context.MovieActors
            .Include(ma => ma.Actor)
            .Where(ma => ma.MovieId == movieId)
            .OrderBy(ma => ma.Actor!.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MovieActor>> GetByActorIdAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        return await _context.MovieActors
            .Include(ma => ma.Movie)
            .Where(ma => ma.ActorId == actorId)
            .OrderByDescending(ma => ma.Movie!.ReleaseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MovieActor movieActor, CancellationToken cancellationToken = default)
    {
        await _context.MovieActors.AddAsync(movieActor, cancellationToken);
    }

    public async Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movieActor = await _context.MovieActors.FindAsync(new object[] { id }, cancellationToken);
        if (movieActor == null)
            return false;

        _context.MovieActors.Remove(movieActor);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default)
    {
        return await _context.MovieActors
            .AnyAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId, cancellationToken);
    }
}
