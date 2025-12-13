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
    private readonly ApplicationDbContext _context;

    public ScreenService(ScreenRepository screenRepository, CinemaRepository cinemaRepository, ApplicationDbContext context)
    {
        _screenRepository = screenRepository;
        _cinemaRepository = cinemaRepository;
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

    private static ScreenResponseDto MapToResponseDto(Screen screen)
    {
        return new ScreenResponseDto
        {
            Id = screen.Id,
            CinemaId = screen.CinemaId,
            Name = screen.Name,
            TotalSeats = screen.TotalSeats,
            SeatMapLayout = screen.SeatMapLayout?.RootElement.GetRawText()
        };
    }
}

