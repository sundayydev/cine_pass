using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Showtime;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class ShowtimeService
{
    private readonly ShowtimeRepository _showtimeRepository;
    private readonly MovieRepository _movieRepository;
    private readonly ApplicationDbContext _context;

    public ShowtimeService(ShowtimeRepository showtimeRepository, MovieRepository movieRepository, ApplicationDbContext context)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _context = context;
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
            IsActive = dto.IsActive
        };

        await _showtimeRepository.AddAsync(showtime, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(showtime);
    }

    public async Task<ShowtimeResponseDto?> UpdateAsync(Guid id, ShowtimeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken);
        if (showtime == null)
            return null;

        if (dto.StartTime.HasValue)
        {
            var movie = await _movieRepository.GetByIdAsync(showtime.MovieId, cancellationToken);
            if (movie == null)
                throw new InvalidOperationException($"Không tìm thấy phim có ID {showtime.MovieId}");

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

        return MapToResponseDto(showtime);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _showtimeRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
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

