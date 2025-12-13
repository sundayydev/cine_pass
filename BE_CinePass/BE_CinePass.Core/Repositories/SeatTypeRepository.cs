using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class SeatTypeRepository : BaseRepository<SeatType>, ISeatTypeRepository
{
    public SeatTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SeatType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(st => st.Code == code, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(st => st.Code == code, cancellationToken);
    }
}

