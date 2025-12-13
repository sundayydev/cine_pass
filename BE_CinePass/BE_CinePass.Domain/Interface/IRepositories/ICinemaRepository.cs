using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho Cinema với các phương thức đặc thù
/// </summary>
public interface ICinemaRepository : IRepository<Cinema>
{
    Task<IEnumerable<Cinema>> GetActiveCinemasAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Cinema>> GetByCityAsync(string city, CancellationToken cancellationToken = default);
    Task<Cinema?> GetWithScreensAsync(Guid cinemaId, CancellationToken cancellationToken = default);
}

