using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho Screen với các phương thức đặc thù
/// </summary>
public interface IScreenRepository : IRepository<Screen>
{
    Task<IEnumerable<Screen>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken cancellationToken = default);
    Task<Screen?> GetWithSeatsAsync(Guid screenId, CancellationToken cancellationToken = default);
}

