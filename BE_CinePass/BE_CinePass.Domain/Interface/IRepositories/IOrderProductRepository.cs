using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho OrderProduct với các phương thức đặc thù
/// </summary>
public interface IOrderProductRepository : IRepository<OrderProduct>
{
    Task<IEnumerable<OrderProduct>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderProduct>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}

