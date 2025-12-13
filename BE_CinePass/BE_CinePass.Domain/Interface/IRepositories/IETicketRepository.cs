using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface.IRepositories;

/// <summary>
/// Repository interface cho ETicket với các phương thức đặc thù
/// </summary>
public interface IETicketRepository : IRepository<ETicket>
{
    Task<ETicket?> GetByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<ETicket>> GetByOrderTicketIdAsync(Guid orderTicketId, CancellationToken cancellationToken = default);
    Task<bool> IsTicketUsedAsync(string ticketCode, CancellationToken cancellationToken = default);
    Task<bool> ValidateTicketAsync(string ticketCode, CancellationToken cancellationToken = default);
}

