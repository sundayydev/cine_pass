using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class OrderProductRepository : BaseRepository<OrderProduct>, IOrderProductRepository
{
    public OrderProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderProduct>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(op => op.OrderId == orderId)
            .Include(op => op.Product)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderProduct>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(op => op.ProductId == productId)
            .Include(op => op.Order)
            .ToListAsync(cancellationToken);
    }
}

