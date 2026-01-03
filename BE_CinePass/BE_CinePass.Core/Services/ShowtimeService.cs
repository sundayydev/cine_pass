using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Showtime;
using BE_CinePass.Shared.DTOs.Seat;
using BE_CinePass.Shared.Common;
using BE_CinePass.Domain.Common;
using BE_CinePass.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class ShowtimeService
{
    private readonly ShowtimeRepository _showtimeRepository;
    private readonly MovieRepository _movieRepository;
    private readonly SeatRepository _seatRepository;
    private readonly SeatTypeRepository _seatTypeRepository;
    private readonly SeatHoldService _seatHoldService;
    private readonly ApplicationDbContext _context;
    private readonly IEventBus _eventBus;
    
    public ShowtimeService(
        ShowtimeRepository showtimeRepository,
        MovieRepository movieRepository,
        SeatRepository seatRepository,
        SeatTypeRepository seatTypeRepository,
        SeatHoldService seatHoldService,
        ApplicationDbContext context,
        IEventBus eventBus)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _seatRepository = seatRepository;
        _seatTypeRepository = seatTypeRepository;
        _seatHoldService = seatHoldService;
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<List<ShowtimeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var showtimes = await _showtimeRepository.GetAllAsync(cancellationToken);
        return showtimes.Select(MapToResponseDto).ToList();
    }

    public async Task<ShowtimeResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken);
        return showtime == null ? null : MapToResponseDto(showtime);
    }

    public async Task<List<ShowtimeResponseDto>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var showtimes = await _showtimeRepository.GetByMovieIdAsync(movieId, cancellationToken);
        return showtimes.Select(MapToResponseDto).ToList();
    }

    public async Task<List<ShowtimeResponseDto>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var showtimes = await _showtimeRepository.GetByDateAsync(date, cancellationToken);
        return showtimes.Select(MapToResponseDto).ToList();
    }

    public async Task<List<ShowtimeResponseDto>> GetByMovieAndDateAsync(Guid movieId, DateTime date, CancellationToken cancellationToken = default)
    {
        var showtimes = await _showtimeRepository.GetByMovieAndDateAsync(movieId, date, cancellationToken);
        return showtimes.Select(MapToResponseDto).ToList();
    }

    public async Task<ShowtimeResponseDto> CreateAsync(ShowtimeCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validate movie exists
        var movie = await _movieRepository.GetByIdAsync(dto.MovieId, cancellationToken);
        if (movie == null)
            throw new InvalidOperationException($"Không tìm thấy phim có ID {dto.MovieId}");

        // Calculate end time
        var endTime = dto.StartTime.AddMinutes(movie.DurationMinutes + 15); // 15 minutes for cleaning

        // Check for overlapping showtimes
        if (await _showtimeRepository.HasOverlappingShowtimeAsync(dto.ScreenId, dto.StartTime, endTime, null, cancellationToken))
            throw new InvalidOperationException("Giờ chiếu phim trùng với giờ chiếu phim hiện có.");

        var showtime = new Showtime
        {
            MovieId = dto.MovieId,
            ScreenId = dto.ScreenId,
            StartTime = dto.StartTime,
            EndTime = endTime,
            BasePrice = dto.BasePrice,
        };

        await _showtimeRepository.AddAsync(showtime, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        var screen = await _context.Screens
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == dto.ScreenId, cancellationToken);

        await _eventBus.PublishAsync(new ShowtimeCreatedEvent
        {
            ShowtimeId = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = movie.Title,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            ScreenId = showtime.ScreenId,
            ScreenName = screen?.Name ?? "N/A",
            CinemaId = screen?.CinemaId ?? Guid.Empty,
            CinemaName = screen?.Cinema?.Name ?? "N/A"
        });
        return MapToResponseDto(showtime);
    }

    public async Task<ShowtimeResponseDto?> UpdateAsync(Guid id, ShowtimeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken);
        if (showtime == null)
            return null;
        
        bool timeChanged = false;
        DateTime oldStartTime = showtime.StartTime;

        if (dto.StartTime.HasValue)
        {
            var movie = await _movieRepository.GetByIdAsync(showtime.MovieId, cancellationToken);
            if (movie == null)
                throw new InvalidOperationException($"Không tìm thấy phim có ID {showtime.MovieId}");
            if (dto.StartTime.Value != showtime.StartTime)
                timeChanged = true;
            
            showtime.StartTime = dto.StartTime.Value;
            showtime.EndTime = dto.EndTime ?? dto.StartTime.Value.AddMinutes(movie.DurationMinutes + 15);

            // Check for overlapping
            if (await _showtimeRepository.HasOverlappingShowtimeAsync(showtime.ScreenId, showtime.StartTime, showtime.EndTime, id, cancellationToken))
                throw new InvalidOperationException("Giờ chiếu phim trùng với giờ chiếu phim hiện có.");
        }
        else if (dto.EndTime.HasValue)
        {
            showtime.EndTime = dto.EndTime.Value;
        }

        if (dto.BasePrice.HasValue)
            showtime.BasePrice = dto.BasePrice.Value;

        if (dto.IsActive.HasValue)
            showtime.IsActive = dto.IsActive.Value;

        _showtimeRepository.Update(showtime);
        await _context.SaveChangesAsync(cancellationToken);
        if (timeChanged)
        {
            var movie = await _movieRepository.GetByIdAsync(showtime.MovieId, cancellationToken);
            var screen = await _context.Screens
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == showtime.ScreenId, cancellationToken);

            await _eventBus.PublishAsync(new ShowtimeTimeChangedEvent
            {
                ShowtimeId = showtime.Id,
                MovieTitle = movie?.Title ?? "N/A",
                OldStartTime = oldStartTime,
                NewStartTime = showtime.StartTime,
                ScreenName = screen?.Name ?? "N/A",
                CinemaName = screen?.Cinema?.Name ?? "N/A"
            });
        }

        return MapToResponseDto(showtime);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken);
        if (showtime == null)
            return false;

        // Get related data before deletion for notification
        var movie = await _movieRepository.GetByIdAsync(showtime.MovieId, cancellationToken);
        var screen = await _context.Screens
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == showtime.ScreenId, cancellationToken);

        var result = await _showtimeRepository.RemoveByIdAsync(id, cancellationToken);
        
        if (result)
        {
            await _context.SaveChangesAsync(cancellationToken);

            // ✅ NEW: Publish ShowtimeCancelledEvent
            await _eventBus.PublishAsync(new ShowtimeCancelledEvent
            {
                ShowtimeId = id,
                MovieTitle = movie?.Title ?? "N/A",
                OriginalStartTime = showtime.StartTime,
                ScreenName = screen?.Name ?? "N/A",
                CinemaName = screen?.Cinema?.Name ?? "N/A",
                Reason = "Cancelled by admin"
            });
        }

        return result;
    }

    public async Task<ShowtimeResponseDto?> UpdatePriceAsync(Guid id, ShowtimeUpdatePriceDto dto, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken);
        if (showtime == null)
            return null;

        showtime.BasePrice = dto.BasePrice;

        _showtimeRepository.Update(showtime);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(showtime);
    }

    public async Task<ShowtimeSeatsResponseDto?> GetSeatsWithStatusAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        // Get showtime with related data
        var showtime = await _context.Showtimes
            .Include(s => s.Screen)
            .FirstOrDefaultAsync(s => s.Id == showtimeId, cancellationToken);

        if (showtime == null)
            return null;

        // Get all seats for the screen
        var seats = await _seatRepository.GetByScreenIdAsync(showtime.ScreenId, cancellationToken);

        // Get sold seats (seats with confirmed orders)
        var soldSeatIds = await _context.OrderTickets
            .Where(ot => ot.ShowtimeId == showtimeId &&
                         ot.Order.Status == Domain.Common.OrderStatus.Confirmed)
            .Select(ot => ot.SeatId)
            .ToListAsync(cancellationToken);

        // Get all seat IDs to check for holds
        var allSeatIds = seats.Select(s => s.Id).ToList();
        var heldSeats = await _seatHoldService.GetHeldSeatsAsync(showtimeId, allSeatIds, cancellationToken);

        // Get seat types for pricing
        var seatTypes = await _seatTypeRepository.GetAllAsync(cancellationToken);
        var seatTypeDict = seatTypes.ToDictionary(st => st.Code, st => st);

        // Map seats with status
        var seatsWithStatus = seats.Select(seat =>
        {
            SeatStatus status;
            string? heldByUserId = null;

            if (soldSeatIds.Contains(seat.Id))
            {
                status = SeatStatus.Sold;
            }
            else if (heldSeats.TryGetValue(seat.Id, out var userId))
            {
                status = SeatStatus.Holding;
                heldByUserId = userId;
            }
            else
            {
                status = SeatStatus.Available;
            }

            // Calculate price based on seat type
            decimal price = showtime.BasePrice;
            if (!string.IsNullOrEmpty(seat.SeatTypeCode) &&
                seatTypeDict.TryGetValue(seat.SeatTypeCode, out var seatType))
            {
                price = showtime.BasePrice * seatType.SurchargeRate;
            }

            return new SeatWithStatusDto
            {
                Id = seat.Id,
                SeatRow = seat.SeatRow,
                SeatNumber = seat.SeatNumber,
                SeatCode = seat.SeatCode,
                SeatTypeCode = seat.SeatTypeCode,
                Status = status,
                Price = price,
                HeldByUserId = heldByUserId
            };
        }).OrderBy(s => s.SeatRow).ThenBy(s => s.SeatNumber).ToList();

        // Calculate statistics
        var availableCount = seatsWithStatus.Count(s => s.Status == SeatStatus.Available);
        var soldCount = seatsWithStatus.Count(s => s.Status == SeatStatus.Sold);
        var holdingCount = seatsWithStatus.Count(s => s.Status == SeatStatus.Holding);

        return new ShowtimeSeatsResponseDto
        {
            ShowtimeId = showtime.Id,
            ScreenId = showtime.ScreenId,
            ScreenName = showtime.Screen.Name,
            ShowDateTime = showtime.StartTime,
            Seats = seatsWithStatus,
            TotalSeats = seatsWithStatus.Count,
            AvailableSeats = availableCount,
            SoldSeats = soldCount,
            HoldingSeats = holdingCount
        };
    }
    
    public async Task<bool> CancelShowtimeAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken);
        if (showtime == null)
            return false;

        if (!showtime.IsActive)
            return false; // Already cancelled

        // Get related data for notification
        var movie = await _movieRepository.GetByIdAsync(showtime.MovieId, cancellationToken);
        var screen = await _context.Screens
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == showtime.ScreenId, cancellationToken);

        // Soft delete
        showtime.IsActive = false;
        _showtimeRepository.Update(showtime);
        await _context.SaveChangesAsync(cancellationToken);

        // ✅ Publish ShowtimeCancelledEvent
        await _eventBus.PublishAsync(new ShowtimeCancelledEvent
        {
            ShowtimeId = id,
            MovieTitle = movie?.Title ?? "N/A",
            OriginalStartTime = showtime.StartTime,
            ScreenName = screen?.Name ?? "N/A",
            CinemaName = screen?.Cinema?.Name ?? "N/A",
            Reason = reason
        });

        return true;
    }

    private static ShowtimeResponseDto MapToResponseDto(Showtime showtime)
    {
        return new ShowtimeResponseDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            ScreenId = showtime.ScreenId,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            BasePrice = showtime.BasePrice,
            IsActive = showtime.IsActive
        };
    }
}

