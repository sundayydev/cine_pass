using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Seat;

namespace BE_CinePass.Core.Services;

public class SeatService
{
    private readonly SeatRepository _seatRepository;
    private readonly ScreenRepository _screenRepository;
    private readonly SeatTypeRepository _seatTypeRepository;
    private readonly ApplicationDbContext _context;

    public SeatService(SeatRepository seatRepository, ScreenRepository screenRepository, SeatTypeRepository seatTypeRepository, ApplicationDbContext context)
    {
        _seatRepository = seatRepository;
        _screenRepository = screenRepository;
        _seatTypeRepository = seatTypeRepository;
        _context = context;
    }

    public async Task<SeatResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var seat = await _seatRepository.GetByIdAsync(id, cancellationToken);
        return seat == null ? null : MapToResponseDto(seat);
    }

    public async Task<List<SeatResponseDto>> GetByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        var seats = await _seatRepository.GetByScreenIdAsync(screenId, cancellationToken);
        return seats.Select(MapToResponseDto).ToList();
    }

    public async Task<List<SeatResponseDto>> GetActiveSeatsByScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        var seats = await _seatRepository.GetActiveSeatsByScreenIdAsync(screenId, cancellationToken);
        return seats.Select(MapToResponseDto).ToList();
    }

    public async Task<SeatResponseDto> CreateAsync(SeatCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validate screen exists
        var screen = await _screenRepository.GetByIdAsync(dto.ScreenId, cancellationToken);
        if (screen == null)
            throw new InvalidOperationException($"Không tìm thấy màn hình có ID {dto.ScreenId}");

        // Check if seat code already exists in this screen
        var existingSeat = await _seatRepository.GetBySeatCodeAsync(dto.ScreenId, dto.SeatCode, cancellationToken);
        if (existingSeat != null)
            throw new InvalidOperationException($"Mã ghế {dto.SeatCode} đã tồn tại trên màn hình này.");

        // Validate seat type if provided
        if (!string.IsNullOrEmpty(dto.SeatTypeCode))
        {
            var seatType = await _seatTypeRepository.GetByCodeAsync(dto.SeatTypeCode, cancellationToken);
            if (seatType == null)
                throw new InvalidOperationException($"Không tìm thấy loại ghế có mã {dto.SeatTypeCode}");
        }

        var seat = new Seat
        {
            ScreenId = dto.ScreenId,
            SeatRow = dto.SeatRow,
            SeatNumber = dto.SeatNumber,
            SeatCode = dto.SeatCode,
            SeatTypeCode = dto.SeatTypeCode,
            IsActive = dto.IsActive
        };

        await _seatRepository.AddAsync(seat, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(seat);
    }

    public async Task<SeatResponseDto?> UpdateAsync(Guid id, SeatUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var seat = await _seatRepository.GetByIdAsync(id, cancellationToken);
        if (seat == null)
            return null;

        if (dto.SeatTypeCode != null)
        {
            if (!string.IsNullOrEmpty(dto.SeatTypeCode))
            {
                var seatType = await _seatTypeRepository.GetByCodeAsync(dto.SeatTypeCode, cancellationToken);
                if (seatType == null)
                    throw new InvalidOperationException($"Không tìm thấy loại ghế có mã {dto.SeatTypeCode}");
            }
            seat.SeatTypeCode = dto.SeatTypeCode;
        }

        if (dto.IsActive.HasValue)
            seat.IsActive = dto.IsActive.Value;

        _seatRepository.Update(seat);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(seat);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _seatRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<bool> IsSeatAvailableAsync(Guid seatId, Guid showtimeId, CancellationToken cancellationToken = default)
    {
        return await _seatRepository.IsSeatAvailableAsync(seatId, showtimeId, cancellationToken);
    }

    private static SeatResponseDto MapToResponseDto(Seat seat)
    {
        return new SeatResponseDto
        {
            Id = seat.Id,
            ScreenId = seat.ScreenId,
            SeatRow = seat.SeatRow,
            SeatNumber = seat.SeatNumber,
            SeatCode = seat.SeatCode,
            SeatTypeCode = seat.SeatTypeCode,
            QrOrderingCode = seat.QrOrderingCode,
            IsActive = seat.IsActive
        };
    }
}

