using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Screen;
using System.Text.Json;

namespace BE_CinePass.Core.Services;

public class ScreenService
{
    private readonly ScreenRepository _screenRepository;
    private readonly CinemaRepository _cinemaRepository;
    private readonly SeatRepository _seatRepository;
    private readonly ApplicationDbContext _context;

    public ScreenService(ScreenRepository screenRepository, CinemaRepository cinemaRepository, SeatRepository seatRepository, ApplicationDbContext context)
    {
        _screenRepository = screenRepository;
        _cinemaRepository = cinemaRepository;
        _seatRepository = seatRepository;
        _context = context;
    }

    public async Task<ScreenResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var screen = await _screenRepository.GetByIdAsync(id, cancellationToken);
        return screen == null ? null : MapToResponseDto(screen);
    }

    public async Task<List<ScreenResponseDto>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken cancellationToken = default)
    {
        var screens = await _screenRepository.GetByCinemaIdAsync(cinemaId, cancellationToken);
        return screens.Select(MapToResponseDto).ToList();
    }

    public async Task<ScreenResponseDto> CreateAsync(ScreenCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validate cinema exists
        var cinema = await _cinemaRepository.GetByIdAsync(dto.CinemaId, cancellationToken);
        if (cinema == null)
            throw new InvalidOperationException($"Không tìm thấy rạp chiếu phim có ID {dto.CinemaId}");

        JsonDocument? seatMapLayout = null;
        if (!string.IsNullOrEmpty(dto.SeatMapLayout))
        {
            try
            {
                seatMapLayout = JsonDocument.Parse(dto.SeatMapLayout);
            }
            catch
            {
                throw new InvalidOperationException("Định dạng JSON không hợp lệ cho SeatMapLayout");
            }
        }

        var screen = new Screen
        {
            CinemaId = dto.CinemaId,
            Name = dto.Name,
            TotalSeats = dto.TotalSeats,
            SeatMapLayout = seatMapLayout
        };

        await _screenRepository.AddAsync(screen, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(screen);
    }

    public async Task<ScreenResponseDto?> UpdateAsync(Guid id, ScreenUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var screen = await _screenRepository.GetByIdAsync(id, cancellationToken);
        if (screen == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            screen.Name = dto.Name;

        if (dto.TotalSeats.HasValue)
            screen.TotalSeats = dto.TotalSeats.Value;

        if (dto.SeatMapLayout != null)
        {
            try
            {
                screen.SeatMapLayout = JsonDocument.Parse(dto.SeatMapLayout);
            }
            catch
            {
                throw new InvalidOperationException("Định dạng JSON không hợp lệ cho SeatMapLayout");
            }
        }

        screen.UpdatedAt = DateTime.UtcNow;
        _screenRepository.Update(screen);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(screen);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _screenRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<object> GenerateSeatsAsync(Guid screenId, GenerateSeatsDto dto, CancellationToken cancellationToken = default)
    {
        // Validate screen exists
        var screen = await _screenRepository.GetByIdAsync(screenId, cancellationToken);
        if (screen == null)
            throw new InvalidOperationException($"Không tìm thấy màn hình có ID {screenId}");

        // Check if seats already exist for this screen
        var existingSeats = await _seatRepository.GetByScreenIdAsync(screenId, cancellationToken);
        if (existingSeats.Any())
            throw new InvalidOperationException("Màn hình này đã có ghế. Vui lòng xóa ghế hiện có trước khi tạo mới.");

        var seats = new List<Seat>();
        var totalSeats = 0;
        var seatTypesByRow = new Dictionary<string, int>(); // Track seat count by type

        // Generate seats: A1, A2, ..., B1, B2, etc.
        for (int row = 0; row < dto.Rows; row++)
        {
            // Convert row number to letter (0 -> A, 1 -> B, etc.)
            string rowLetter = ((char)('A' + row)).ToString();

            // Determine seat type for this row
            string? seatTypeCode = null;
            if (dto.RowSeatTypes != null && dto.RowSeatTypes.ContainsKey(rowLetter))
            {
                seatTypeCode = dto.RowSeatTypes[rowLetter];
            }
            else
            {
                seatTypeCode = dto.DefaultSeatTypeCode;
            }

            for (int seatNum = 1; seatNum <= dto.SeatsPerRow; seatNum++)
            {
                var seatCode = $"{rowLetter}{seatNum}";

                var seat = new Seat
                {
                    ScreenId = screenId,
                    SeatRow = rowLetter,
                    SeatNumber = seatNum,
                    SeatCode = seatCode,
                    SeatTypeCode = seatTypeCode,
                    IsActive = true
                };

                seats.Add(seat);
                totalSeats++;

                // Track seat types
                var typeKey = seatTypeCode ?? "NULL";
                if (!seatTypesByRow.ContainsKey(typeKey))
                    seatTypesByRow[typeKey] = 0;
                seatTypesByRow[typeKey]++;
            }
        }

        // Add all seats to database
        foreach (var seat in seats)
        {
            await _seatRepository.AddAsync(seat, cancellationToken);
        }

        // Update screen's total seats
        screen.TotalSeats = totalSeats;
        screen.UpdatedAt = DateTime.UtcNow;
        _screenRepository.Update(screen);

        await _context.SaveChangesAsync(cancellationToken);

        return new
        {
            ScreenId = screenId,
            TotalSeatsGenerated = totalSeats,
            Rows = dto.Rows,
            SeatsPerRow = dto.SeatsPerRow,
            SeatTypeBreakdown = seatTypesByRow,
            Message = $"Đã tạo thành công {totalSeats} ghế cho màn hình {screen.Name}"
        };
    }

    private static ScreenResponseDto MapToResponseDto(Screen screen)
    {
        return new ScreenResponseDto
        {
            Id = screen.Id,
            CinemaId = screen.CinemaId,
            Name = screen.Name,
            TotalSeats = screen.TotalSeats,
            SeatMapLayout = screen.SeatMapLayout?.RootElement.GetRawText(),
            CreatedAt = screen.CreatedAt,
            UpdatedAt = screen.UpdatedAt,
        };
    }
}

