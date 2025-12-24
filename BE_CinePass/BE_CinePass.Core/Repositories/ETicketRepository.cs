using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Interface.IRepositories;
using BE_CinePass.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Repositories;

public class ETicketRepository : BaseRepository<ETicket>, IETicketRepository
{
    public ETicketRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ETicket?> GetByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Showtime)
                    .ThenInclude(st => st.Movie)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Showtime)
                    .ThenInclude(st => st.Screen)
                        .ThenInclude(sc => sc.Cinema)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Seat)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Order)
                    .ThenInclude(o => o.User)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Order)
                    .ThenInclude(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(et => et.TicketCode == ticketCode, cancellationToken);
    }

    public async Task<IEnumerable<ETicket>> GetByOrderTicketIdAsync(Guid orderTicketId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(et => et.OrderTicketId == orderTicketId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ETicket?> GetByQrDataAsync(string qrData, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Showtime)
                    .ThenInclude(st => st.Movie)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Showtime)
                    .ThenInclude(st => st.Screen)
                        .ThenInclude(sc => sc.Cinema)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Seat)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Order)
                    .ThenInclude(o => o.User)
            .Include(et => et.OrderTicket)
                .ThenInclude(ot => ot.Order)
                    .ThenInclude(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(et => et.QrData == qrData, cancellationToken);
    }

    public async Task<bool> IsTicketUsedAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        var ticket = await GetByTicketCodeAsync(ticketCode, cancellationToken);
        return ticket?.IsUsed ?? false;
    }

    public async Task<bool> ValidateTicketAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        var ticket = await GetByTicketCodeAsync(ticketCode, cancellationToken);
        return ticket != null && !ticket.IsUsed;
    }
}

