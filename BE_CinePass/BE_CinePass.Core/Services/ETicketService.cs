using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.ETicket;
using Microsoft.EntityFrameworkCore;

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

    /// <summary>
    /// Lấy tất cả vé của user trong 1 query duy nhất (tối ưu performance)
    /// </summary>
    public async Task<List<MyTicketDto>> GetMyTicketsAsync(Guid userId, bool upcomingOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ETickets
            .Include(e => e.OrderTicket)
                .ThenInclude(ot => ot.Order)
            .Include(e => e.OrderTicket)
                .ThenInclude(ot => ot.Showtime)
                    .ThenInclude(s => s.Movie)
            .Include(e => e.OrderTicket)
                .ThenInclude(ot => ot.Showtime)
                    .ThenInclude(s => s.Screen)
                        .ThenInclude(sc => sc.Cinema)
            .Include(e => e.OrderTicket)
                .ThenInclude(ot => ot.Seat)
            .Where(e => e.OrderTicket.Order.UserId == userId 
                     && e.OrderTicket.Order.Status == Domain.Common.OrderStatus.Confirmed)
            .AsQueryable();

        // Nếu chỉ lấy vé sắp chiếu
        if (upcomingOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(e => !e.IsUsed && e.OrderTicket.Showtime.StartTime > now);
        }

        var tickets = await query
            .OrderByDescending(e => e.OrderTicket.Showtime.StartTime)
            .Select(e => new MyTicketDto
            {
                Id = e.Id,
                TicketCode = e.TicketCode,
                QrData = e.QrData,
                IsUsed = e.IsUsed,
                UsedAt = e.UsedAt,
                CreatedAt = e.OrderTicket.Order.CreatedAt,
                
                // Movie info
                MovieTitle = e.OrderTicket.Showtime.Movie.Title,
                MoviePosterUrl = e.OrderTicket.Showtime.Movie.PosterUrl,
                MovieDurationMinutes = e.OrderTicket.Showtime.Movie.DurationMinutes,
                MovieCategory = e.OrderTicket.Showtime.Movie.Category.ToString(),
                MovieAgeLimit = e.OrderTicket.Showtime.Movie.AgeLimit,
                
                // Showtime info
                ShowtimeStart = e.OrderTicket.Showtime.StartTime,
                ShowtimeEnd = e.OrderTicket.Showtime.EndTime,
                
                // Cinema & Screen info
                CinemaName = e.OrderTicket.Showtime.Screen.Cinema.Name,
                CinemaAddress = e.OrderTicket.Showtime.Screen.Cinema.Address,
                ScreenName = e.OrderTicket.Showtime.Screen.Name,
                
                // Seat info
                SeatCode = e.OrderTicket.Seat.SeatCode,
                SeatType = e.OrderTicket.Seat.SeatTypeCode
            })
            .ToListAsync(cancellationToken);

        return tickets;
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

    /// <summary>
    /// Tạo ETickets cho tất cả OrderTickets đã confirmed nhưng chưa có ETicket (fix data cũ)
    /// </summary>
    public async Task<int> GenerateMissingETicketsAsync(CancellationToken cancellationToken = default)
    {
        // Lấy tất cả OrderTickets thuộc orders đã Confirmed nhưng chưa có ETicket
        var orderTicketsWithoutETicket = await _context.OrderTickets
            .Include(ot => ot.Order)
            .Where(ot => ot.Order.Status == Domain.Common.OrderStatus.Confirmed)
            .Where(ot => !_context.ETickets.Any(e => e.OrderTicketId == ot.Id))
            .ToListAsync(cancellationToken);

        int generatedCount = 0;

        foreach (var orderTicket in orderTicketsWithoutETicket)
        {
            try
            {
                var ticketCode = GenerateTicketCode();
                var qrData = GenerateQrData(ticketCode, orderTicket);

                var eTicket = new ETicket
                {
                    OrderTicketId = orderTicket.Id,
                    TicketCode = ticketCode,
                    QrData = qrData,
                    IsUsed = false
                };

                await _eTicketRepository.AddAsync(eTicket, cancellationToken);
                generatedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating e-ticket for orderTicket {orderTicket.Id}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return generatedCount;
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

    public async Task<TicketVerificationResultDto> VerifyAndUseTicketAsync(string qrData, CancellationToken cancellationToken = default)
    {
        // Find ticket by QR data
        var eTicket = await _eTicketRepository.GetByQrDataAsync(qrData, cancellationToken);

        // Ticket not found
        if (eTicket == null)
        {
            return new TicketVerificationResultDto
            {
                IsValid = false,
                Status = "Invalid",
                Message = "Vé không hợp lệ. Không tìm thấy thông tin vé trong hệ thống.",
                TicketDetail = null,
                Summary = null
            };
        }

        // Ticket already used
        if (eTicket.IsUsed)
        {
            return new TicketVerificationResultDto
            {
                IsValid = false,
                Status = "AlreadyUsed",
                Message = $"Vé đã được sử dụng lúc {eTicket.UsedAt:dd/MM/yyyy HH:mm:ss}.",
                CheckinAt = eTicket.UsedAt,
                TicketDetail = MapToDetailDto(eTicket),
                Summary = BuildCheckinSummary(eTicket)
            };
        }

        // Verify order is confirmed
        if (eTicket.OrderTicket?.Order?.Status != Domain.Common.OrderStatus.Confirmed)
        {
            return new TicketVerificationResultDto
            {
                IsValid = false,
                Status = "Invalid",
                Message = "Đơn hàng chưa được xác nhận thanh toán.",
                TicketDetail = null,
                Summary = null
            };
        }

        // Check if showtime has expired (more than 30 minutes after start time)
        var showtime = eTicket.OrderTicket?.Showtime;
        if (showtime != null && DateTime.UtcNow > showtime.StartTime.AddMinutes(30))
        {
            return new TicketVerificationResultDto
            {
                IsValid = false,
                Status = "Expired",
                Message = "Vé đã hết hạn. Suất chiếu đã bắt đầu quá 30 phút.",
                TicketDetail = MapToDetailDto(eTicket),
                Summary = BuildCheckinSummary(eTicket)
            };
        }

        // Mark ticket as used
        var checkinTime = DateTime.UtcNow;
        eTicket.IsUsed = true;
        eTicket.UsedAt = checkinTime;

        _eTicketRepository.Update(eTicket);
        await _context.SaveChangesAsync(cancellationToken);

        // Return success with ticket details and summary
        return new TicketVerificationResultDto
        {
            IsValid = true,
            Status = "Valid",
            Message = "Vé hợp lệ. Check-in thành công!",
            CheckinAt = checkinTime,
            TicketDetail = MapToDetailDto(eTicket),
            Summary = BuildCheckinSummary(eTicket)
        };
    }

    /// <summary>
    /// Xây dựng thông tin tổng hợp nhanh cho việc hiển thị check-in
    /// </summary>
    private CheckinSummaryDto BuildCheckinSummary(ETicket eTicket)
    {
        var orderTicket = eTicket.OrderTicket;
        var showtime = orderTicket?.Showtime;
        var movie = showtime?.Movie;
        var screen = showtime?.Screen;
        var cinema = screen?.Cinema;
        var seat = orderTicket?.Seat;
        var order = orderTicket?.Order;
        var user = order?.User;

        var now = DateTime.UtcNow;
        var minutesUntilShowtime = showtime != null
            ? (int)(showtime.StartTime - now).TotalMinutes
            : 0;

        // Đếm số vé đã check-in trong đơn hàng
        var totalTicketsInOrder = order?.OrderTickets?.Count ?? 1;
        var checkedInTicketsInOrder = order?.OrderTickets?
            .Count(ot => ot.ETickets?.Any(et => et.IsUsed) == true) ?? 0;

        // Lấy thông tin sản phẩm đi kèm
        var products = order?.OrderProducts?
            .Select(op => new OrderProductSummaryDto
            {
                ProductName = op.Product?.Name ?? "N/A",
                Quantity = op.Quantity,
                UnitPrice = op.UnitPrice,
                Category = op.Product?.Category.ToString()
            })
            .ToList();

        return new CheckinSummaryDto
        {
            // Thông tin vé
            TicketCode = eTicket.TicketCode,
            TicketPrice = orderTicket?.Price ?? 0,

            // Thông tin phim
            MovieTitle = movie?.Title ?? "N/A",
            MoviePosterUrl = movie?.PosterUrl,
            MovieDurationMinutes = movie?.DurationMinutes ?? 0,
            MovieRating = movie?.AgeLimit > 0 ? $"{movie.AgeLimit}+" : null, // Phân loại độ tuổi từ AgeLimit

            // Thông tin suất chiếu
            ShowtimeStart = showtime?.StartTime ?? DateTime.MinValue,
            ShowtimeEnd = showtime?.EndTime ?? DateTime.MinValue,
            MinutesUntilShowtime = minutesUntilShowtime,
            IsShowtimeStarted = minutesUntilShowtime < 0,

            // Thông tin rạp & phòng chiếu
            CinemaName = cinema?.Name ?? "N/A",
            CinemaAddress = cinema?.Address,
            ScreenName = screen?.Name ?? "N/A",

            // Thông tin ghế
            SeatCode = seat?.SeatCode ?? "N/A",
            SeatRow = seat?.SeatRow ?? "",
            SeatNumber = seat?.SeatNumber ?? 0,
            SeatType = seat?.SeatTypeCode,

            // Thông tin khách hàng
            CustomerName = user?.FullName,
            CustomerEmail = user?.Email,
            CustomerPhone = user?.Phone,

            // Thông tin đơn hàng
            OrderId = order?.Id ?? Guid.Empty,
            OrderTotalAmount = order?.TotalAmount ?? 0,
            TotalTicketsInOrder = totalTicketsInOrder,
            CheckedInTicketsInOrder = checkedInTicketsInOrder,

            // Sản phẩm đi kèm
            Products = products
        };
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
        var screen = eTicket.OrderTicket?.Showtime?.Screen;
        var cinema = screen?.Cinema;
        var seat = eTicket.OrderTicket?.Seat;
        var user = eTicket.OrderTicket?.Order?.User;

        return new ETicketDetailDto
        {
            Id = eTicket.Id,
            TicketCode = eTicket.TicketCode,
            QrData = eTicket.QrData,
            IsUsed = eTicket.IsUsed,
            UsedAt = eTicket.UsedAt,

            // Thông tin rạp chiếu
            Cinema = cinema != null ? new CinemaInfoDto
            {
                Id = cinema.Id,
                Name = cinema.Name,
                Address = cinema.Address,
                City = cinema.City,
                Phone = cinema.Phone
            } : null,

            // Thông tin phòng chiếu
            Screen = screen != null ? new ScreenInfoDto
            {
                Id = screen.Id,
                Name = screen.Name,
                TotalSeats = screen.TotalSeats
            } : null,

            // Thông tin ghế
            Seat = seat != null ? new SeatInfoDto
            {
                Id = seat.Id,
                SeatCode = seat.SeatCode,
                SeatRow = seat.SeatRow,
                SeatNumber = seat.SeatNumber,
                SeatTypeCode = seat.SeatTypeCode
            } : null,

            // Thông tin người mua vé
            Buyer = user != null ? new BuyerInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone
            } : null,

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
                    Screen = screen != null ? new Shared.DTOs.Screen.ScreenResponseDto
                    {
                        Id = screen.Id,
                        CinemaId = screen.CinemaId,
                        CinemaName = cinema?.Name,
                        Name = screen.Name,
                        TotalSeats = screen.TotalSeats,
                        SeatMapLayout = screen.SeatMapLayout?.RootElement.GetRawText()
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

