using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho SeatType với các phương thức đặc thù
/// </summary>
public interface ISeatTypeRepository : IRepository<SeatType>
{
    Task<SeatType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}

