using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho PaymentTransaction với các phương thức đặc thù
/// </summary>
public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
{
    Task<IEnumerable<PaymentTransaction>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaymentTransaction?> GetByProviderTransIdAsync(string providerTransId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentTransaction>> GetSuccessfulTransactionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentTransaction>> GetFailedTransactionsAsync(CancellationToken cancellationToken = default);
}

