using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class ActorRepository
{
    private readonly ApplicationDbContext _context;

    public ActorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Actor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Actors
            .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Actor?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Actors
            .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);
    }

    public async Task<List<Actor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Actors
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Actor>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context.Actors
            .Where(a => a.Name.ToLower().Contains(lowerSearchTerm) ||
                       (a.Description != null && a.Description.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Actor actor, CancellationToken cancellationToken = default)
    {
        await _context.Actors.AddAsync(actor, cancellationToken);
    }

    public void Update(Actor actor)
    {
        _context.Actors.Update(actor);
    }

    public async Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await _context.Actors.FindAsync(new object[] { id }, cancellationToken);
        if (actor == null)
            return false;

        _context.Actors.Remove(actor);
        return true;
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Actors
            .AnyAsync(a => a.Slug == slug, cancellationToken);
    }
}
