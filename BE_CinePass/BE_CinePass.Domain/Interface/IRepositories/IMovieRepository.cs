using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho Movie với các phương thức đặc thù
/// </summary>
public interface IMovieRepository : IRepository<Movie>
{
    Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetByStatusAsync(MovieStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetNowShowingAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetComingSoonAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}

