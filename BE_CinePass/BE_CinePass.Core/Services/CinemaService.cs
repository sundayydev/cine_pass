using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Cinema;

namespace BE_CinePass.Core.Services;

public class CinemaService
{
    private readonly CinemaRepository _cinemaRepository;
    private readonly ApplicationDbContext _context;

    public CinemaService(CinemaRepository cinemaRepository, ApplicationDbContext context)
    {
        _cinemaRepository = cinemaRepository;
        _context = context;
    }

    public async Task<CinemaResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(id, cancellationToken);
        return cinema == null ? null : MapToResponseDto(cinema);
    }

    public async Task<List<CinemaResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cinemas = await _cinemaRepository.GetAllAsync(cancellationToken);
        return cinemas.Select(MapToResponseDto).ToList();
    }

    public async Task<List<CinemaResponseDto>> GetActiveCinemasAsync(CancellationToken cancellationToken = default)
    {
        var cinemas = await _cinemaRepository.GetActiveCinemasAsync(cancellationToken);
        return cinemas.Select(MapToResponseDto).ToList();
    }

    public async Task<List<CinemaResponseDto>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        var cinemas = await _cinemaRepository.GetByCityAsync(city, cancellationToken);
        return cinemas.Select(MapToResponseDto).ToList();
    }

    public async Task<CinemaResponseDto> CreateAsync(CinemaCreateDto dto, CancellationToken cancellationToken = default)
    {
        var cinema = new Cinema
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            BannerUrl = dto.BannerUrl,
            TotalScreens = dto.TotalScreens,
            Facilities = dto.Facilities,
            IsActive = dto.IsActive
        };

        await _cinemaRepository.AddAsync(cinema, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(cinema);
    }

    public async Task<CinemaResponseDto?> UpdateAsync(Guid id, CinemaUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(id, cancellationToken);
        if (cinema == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            cinema.Name = dto.Name;

        if (dto.Address != null)
            cinema.Address = dto.Address;

        if (dto.City != null)
            cinema.City = dto.City;
        
        if (dto.Phone != null)
            cinema.Phone = dto.Phone;
        
        if (dto.Email != null)
            cinema.Email = dto.Email;
        
        if (dto.Website != null)
            cinema.Website = dto.Website;
        
        if (dto.Latitude != null)
            cinema.Latitude = dto.Latitude;
        
        if (dto.Longitude != null)
            cinema.Longitude = dto.Longitude;
        
        if (dto.BannerUrl != null)
            cinema.BannerUrl = dto.BannerUrl;
        
        if (dto.TotalScreens.HasValue)
            cinema.TotalScreens = dto.TotalScreens.Value;
        
        if (dto.Facilities != null)
            cinema.Facilities = dto.Facilities;
        
        if (dto.Slug != null)
            cinema.Slug = dto.Slug;
        
        if (dto.Description != null)
            cinema.Description = dto.Description;
        
        if (dto.IsActive.HasValue)
            cinema.IsActive = dto.IsActive.Value;

        cinema.UpdatedAt = DateTime.UtcNow;
        
        _cinemaRepository.Update(cinema);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(cinema);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _cinemaRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static CinemaResponseDto MapToResponseDto(Cinema cinema)
    {
        return new CinemaResponseDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Phone = cinema.Phone,
            Email = cinema.Email,
            Website = cinema.Website,
            Latitude = cinema.Latitude,
            Longitude = cinema.Longitude,
            BannerUrl = cinema.BannerUrl,
            TotalScreens = cinema.TotalScreens,
            Facilities = cinema.Facilities,
            IsActive = cinema.IsActive,
            CreatedAt = cinema.CreatedAt,
            UpdatedAt = cinema.UpdatedAt
        };
    }
}

