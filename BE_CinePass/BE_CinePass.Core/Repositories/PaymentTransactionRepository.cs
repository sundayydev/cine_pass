using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class PaymentTransactionRepository : BaseRepository<PaymentTransaction>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(pt => pt.OrderId == orderId)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentTransaction?> GetByProviderTransIdAsync(string providerTransId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pt => pt.ProviderTransId == providerTransId, cancellationToken);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetSuccessfulTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(pt => pt.Status != null && pt.Status.ToLower() == "success")
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetFailedTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(pt => pt.Status != null && pt.Status.ToLower() == "fail")
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

