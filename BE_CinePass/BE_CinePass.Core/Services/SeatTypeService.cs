using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.SeatType;

namespace BE_CinePass.Core.Services;

public class SeatTypeService
{
    private readonly SeatTypeRepository _seatTypeRepository;
    private readonly ApplicationDbContext _context;

    public SeatTypeService(SeatTypeRepository seatTypeRepository, ApplicationDbContext context)
    {
        _seatTypeRepository = seatTypeRepository;
        _context = context;
    }

    public async Task<SeatTypeResponseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var seatType = await _seatTypeRepository.GetByCodeAsync(code, cancellationToken);
        return seatType == null ? null : MapToResponseDto(seatType);
    }

    public async Task<List<SeatTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var seatTypes = await _seatTypeRepository.GetAllAsync(cancellationToken);
        return seatTypes.Select(MapToResponseDto).ToList();
    }

    public async Task<SeatTypeResponseDto> CreateAsync(SeatTypeCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (await _seatTypeRepository.CodeExistsAsync(dto.Code, cancellationToken))
            throw new InvalidOperationException($"Loại ghế có mã {dto.Code} đã tồn tại");

        var seatType = new SeatType
        {
            Code = dto.Code,
            Name = dto.Name,
            SurchargeRate = dto.SurchargeRate
        };

        await _seatTypeRepository.AddAsync(seatType, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(seatType);
    }

    public async Task<SeatTypeResponseDto?> UpdateAsync(string code, SeatTypeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var seatType = await _seatTypeRepository.GetByCodeAsync(code, cancellationToken);
        if (seatType == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            seatType.Name = dto.Name;

        if (dto.SurchargeRate.HasValue)
            seatType.SurchargeRate = dto.SurchargeRate.Value;

        _seatTypeRepository.Update(seatType);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(seatType);
    }

    public async Task<bool> DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        var seatType = await _seatTypeRepository.GetByCodeAsync(code, cancellationToken);
        if (seatType == null)
            return false;

        _seatTypeRepository.Remove(seatType);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static SeatTypeResponseDto MapToResponseDto(SeatType seatType)
    {
        return new SeatTypeResponseDto
        {
            Code = seatType.Code,
            Name = seatType.Name,
            SurchargeRate = seatType.SurchargeRate
        };
    }
}

