using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.ETicket;

namespace BE_CinePass.Core.Services;

public class ETicketService
{
    private readonly ETicketRepository _eTicketRepository;
    private readonly OrderTicketRepository _orderTicketRepository;
    private readonly ApplicationDbContext _context;

    public ETicketService(ETicketRepository eTicketRepository, OrderTicketRepository orderTicketRepository, ApplicationDbContext context)
    {
        _eTicketRepository = eTicketRepository;
        _orderTicketRepository = orderTicketRepository;
        _context = context;
    }

    public async Task<ETicketResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eTicket = await _eTicketRepository.GetByIdAsync(id, cancellationToken);
        return eTicket == null ? null : MapToResponseDto(eTicket);
    }

    public async Task<ETicketResponseDto?> GetByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        var eTicket = await _eTicketRepository.GetByTicketCodeAsync(ticketCode, cancellationToken);
        return eTicket == null ? null : MapToResponseDto(eTicket);
    }

    public async Task<ETicketDetailDto?> GetDetailByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        var eTicket = await _eTicketRepository.GetByTicketCodeAsync(ticketCode, cancellationToken);
        return eTicket == null ? null : MapToDetailDto(eTicket);
    }

    public async Task<List<ETicketResponseDto>> GetByOrderTicketIdAsync(Guid orderTicketId, CancellationToken cancellationToken = default)
    {
        var eTickets = await _eTicketRepository.GetByOrderTicketIdAsync(orderTicketId, cancellationToken);
        return eTickets.Select(MapToResponseDto).ToList();
    }

    public async Task<ETicketResponseDto> GenerateETicketAsync(Guid orderTicketId, CancellationToken cancellationToken = default)
    {
        // Validate order ticket exists
        var orderTicket = await _orderTicketRepository.GetByIdAsync(orderTicketId, cancellationToken);
        if (orderTicket == null)
            throw new InvalidOperationException($"Order ticket with id {orderTicketId} not found");

        // Check if order is confirmed
        if (orderTicket.Order.Status != Domain.Common.OrderStatus.Confirmed)
            throw new InvalidOperationException("Can only generate e-ticket for confirmed orders");

        // Generate unique ticket code
        var ticketCode = GenerateTicketCode();

        // Generate QR data
        var qrData = GenerateQrData(ticketCode, orderTicket);

        var eTicket = new ETicket
        {
            OrderTicketId = orderTicketId,
            TicketCode = ticketCode,
            QrData = qrData,
            IsUsed = false
        };

        await _eTicketRepository.AddAsync(eTicket, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(eTicket);
    }

    public async Task<bool> ValidateTicketAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        return await _eTicketRepository.ValidateTicketAsync(ticketCode, cancellationToken);
    }

    public async Task<bool> UseTicketAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        var eTicket = await _eTicketRepository.GetByTicketCodeAsync(ticketCode, cancellationToken);
        if (eTicket == null)
            return false;

        if (eTicket.IsUsed)
            throw new InvalidOperationException("Vé đã được sử dụng.");

        eTicket.IsUsed = true;
        eTicket.UsedAt = DateTime.UtcNow;

        _eTicketRepository.Update(eTicket);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string GenerateTicketCode()
    {
        // Generate 8-character alphanumeric code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string GenerateQrData(string ticketCode, OrderTicket orderTicket)
    {
        // Simple QR data format: ticketCode|orderTicketId|timestamp
        return $"{ticketCode}|{orderTicket.Id}|{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private static ETicketResponseDto MapToResponseDto(ETicket eTicket)
    {
        return new ETicketResponseDto
        {
            Id = eTicket.Id,
            OrderTicketId = eTicket.OrderTicketId,
            TicketCode = eTicket.TicketCode,
            QrData = eTicket.QrData,
            IsUsed = eTicket.IsUsed,
            UsedAt = eTicket.UsedAt
        };
    }

    private static ETicketDetailDto MapToDetailDto(ETicket eTicket)
    {
        return new ETicketDetailDto
        {
            Id = eTicket.Id,
            TicketCode = eTicket.TicketCode,
            QrData = eTicket.QrData,
            IsUsed = eTicket.IsUsed,
            UsedAt = eTicket.UsedAt,
            OrderTicket = eTicket.OrderTicket != null ? new Shared.DTOs.Order.OrderTicketDetailDto
            {
                Id = eTicket.OrderTicket.Id,
                ShowtimeId = eTicket.OrderTicket.ShowtimeId,
                SeatId = eTicket.OrderTicket.SeatId,
                Price = eTicket.OrderTicket.Price,
                Showtime = eTicket.OrderTicket.Showtime != null ? new Shared.DTOs.Showtime.ShowtimeDetailDto
                {
                    Id = eTicket.OrderTicket.Showtime.Id,
                    StartTime = eTicket.OrderTicket.Showtime.StartTime,
                    EndTime = eTicket.OrderTicket.Showtime.EndTime,
                    BasePrice = eTicket.OrderTicket.Showtime.BasePrice,
                    IsActive = eTicket.OrderTicket.Showtime.IsActive,
                    Movie = eTicket.OrderTicket.Showtime.Movie != null ? new Shared.DTOs.Movie.MovieResponseDto
                    {
                        Id = eTicket.OrderTicket.Showtime.Movie.Id,
                        Title = eTicket.OrderTicket.Showtime.Movie.Title,
                        Slug = eTicket.OrderTicket.Showtime.Movie.Slug,
                        DurationMinutes = eTicket.OrderTicket.Showtime.Movie.DurationMinutes,
                        Description = eTicket.OrderTicket.Showtime.Movie.Description,
                        PosterUrl = eTicket.OrderTicket.Showtime.Movie.PosterUrl,
                        TrailerUrl = eTicket.OrderTicket.Showtime.Movie.TrailerUrl,
                        ReleaseDate = eTicket.OrderTicket.Showtime.Movie.ReleaseDate,
                        Status = eTicket.OrderTicket.Showtime.Movie.Status.ToString(),
                        CreatedAt = eTicket.OrderTicket.Showtime.Movie.CreatedAt
                    } : null!,
                    Screen = eTicket.OrderTicket.Showtime.Screen != null ? new Shared.DTOs.Screen.ScreenResponseDto
                    {
                        Id = eTicket.OrderTicket.Showtime.Screen.Id,
                        CinemaId = eTicket.OrderTicket.Showtime.Screen.CinemaId,
                        Name = eTicket.OrderTicket.Showtime.Screen.Name,
                        TotalSeats = eTicket.OrderTicket.Showtime.Screen.TotalSeats,
                        SeatMapLayout = eTicket.OrderTicket.Showtime.Screen.SeatMapLayout?.RootElement.GetRawText()
                    } : null!
                } : null,
                Seat = eTicket.OrderTicket.Seat != null ? new Shared.DTOs.Seat.SeatResponseDto
                {
                    Id = eTicket.OrderTicket.Seat.Id,
                    ScreenId = eTicket.OrderTicket.Seat.ScreenId,
                    SeatRow = eTicket.OrderTicket.Seat.SeatRow,
                    SeatNumber = eTicket.OrderTicket.Seat.SeatNumber,
                    SeatCode = eTicket.OrderTicket.Seat.SeatCode,
                    SeatTypeCode = eTicket.OrderTicket.Seat.SeatTypeCode,
                    IsActive = eTicket.OrderTicket.Seat.IsActive
                } : null
            } : null!
        };
    }
}

