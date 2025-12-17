using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Cinema;
using BE_CinePass.Shared.DTOs.Movie;
using BE_CinePass.Shared.DTOs.Showtime;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<CinemaResponseDto>> GetByCityAsync(string city,
        CancellationToken cancellationToken = default)
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
            Description = dto.Description,
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

    public async Task<CinemaResponseDto?> UpdateAsync(Guid id, CinemaUpdateDto dto,
        CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Lấy danh sách các phim đang chiếu tại rạp (có lịch chiếu từ hôm nay trở đi)
    /// </summary>
    public async Task<List<MovieResponseDto>> GetCurrentlyShowingMoviesAsync(Guid cinemaId,
        CancellationToken cancellationToken = default)
    {
        // Lấy các phim có ít nhất 1 suất chiếu active, bắt đầu từ hôm nay trở đi trong vòng 14 ngày (có thể điều chỉnh)
        var today = DateTime.UtcNow.Date;
        var maxFutureDays = today.AddDays(14); // Có thể giảm xuống 7 nếu muốn

        var movies = await _context.Showtimes
            .AsNoTracking()
            .Where(s => s.Screen.CinemaId == cinemaId
                        && s.IsActive
                        && s.StartTime.Date >= today
                        && s.StartTime.Date <= maxFutureDays)
            .Include(s => s.Movie)
            .Select(s => s.Movie)
            .Distinct()
            .OrderBy(m => m.Title)
            .ToListAsync(cancellationToken);

        return movies.Select(m => new MovieResponseDto
        {
            Id = m.Id,
            Title = m.Title,
            Slug = m.Slug,
            Description = m.Description,
            PosterUrl = m.PosterUrl,
            TrailerUrl = m.TrailerUrl,
            DurationMinutes = m.DurationMinutes,
            ReleaseDate = m.ReleaseDate,
            Category = m.Category.ToString(),
            Status = m.Status.ToString(),
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Lấy thông tin chi tiết rạp kèm danh sách phim đang chiếu
    /// </summary>
    public async Task<CinemaDetailResponseDto?> GetCinemaDetailAsync(Guid cinemaId,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var maxFutureDays = today.AddDays(14);

        // Lấy cinema trước
        var cinema = await _cinemaRepository.GetByIdAsync(cinemaId, cancellationToken);
        if (cinema == null)
            return null;

        // Lấy danh sách phim đang chiếu (cùng logic đã sửa ở trên)
        var movies = await _context.Showtimes
            .AsNoTracking()
            .Where(s => s.Screen.CinemaId == cinemaId
                        && s.IsActive
                        && s.EndTime > today)
            .Include(s => s.Movie)
            .Select(s => s.Movie)
            .Distinct()
            .OrderBy(m => m.Title)
            .ToListAsync(cancellationToken);

        var movieDtos = movies.Select(m => new MovieResponseDto
        {
            Id = m.Id,
            Title = m.Title,
            Slug = m.Slug,
            Description = m.Description,
            PosterUrl = m.PosterUrl,
            TrailerUrl = m.TrailerUrl,
            DurationMinutes = m.DurationMinutes,
            ReleaseDate = m.ReleaseDate,
            Category = m.Category.ToString(),
            Status = m.Status.ToString(),
            CreatedAt = m.CreatedAt
        }).ToList();

        return new CinemaDetailResponseDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Slug = cinema.Slug ?? string.Empty,
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
            UpdatedAt = cinema.UpdatedAt,
            CurrentlyShowingMovies = movieDtos
        };
    }


    /// <summary>
    /// Lấy danh sách tất cả rạp kèm phim đang chiếu (chỉ rạp active)
    /// </summary>
    public async Task<List<CinemaDetailResponseDto>> GetAllWithCurrentlyShowingMoviesAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // 1. Lấy tất cả rạp đang hoạt động
        var cinemas = await _cinemaRepository.GetActiveCinemasAsync(cancellationToken);

        // 2. active + suất chưa kết thúc
        var activeShowtimes = await _context.Showtimes
            .AsNoTracking()
            .Where(s => s.IsActive
                        && s.Screen.Cinema.IsActive
                        && s.EndTime > now) // suất chưa kết thúc 
            .Include(s => s.Movie)
            .Include(s => s.Screen)
            .ThenInclude(screen => screen.Cinema)
            .ToListAsync(cancellationToken);

        // 3. Group theo CinemaId 
        var moviesByCinema = activeShowtimes
            .GroupBy(s => s.Screen.CinemaId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => s.Movie)
                    .DistinctBy(m => m.Id) 
                    .OrderBy(m => m.Title)
                    .Select(m => new MovieResponseDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Slug = m.Slug,
                        Description = m.Description,
                        PosterUrl = m.PosterUrl,
                        TrailerUrl = m.TrailerUrl,
                        DurationMinutes = m.DurationMinutes,
                        ReleaseDate = m.ReleaseDate,
                        Category = m.Category.ToString(),
                        Status = m.Status.ToString(), 
                        CreatedAt = m.CreatedAt
                    })
                    .ToList()
            );

        // 4. Map sang DTO response
        var result = cinemas.Select(cinema => new CinemaDetailResponseDto
            {
                Id = cinema.Id,
                Name = cinema.Name,
                Slug = cinema.Slug ?? string.Empty,
                Address = cinema.Address,
                City = cinema.City,
                Phone = cinema.Phone,
                Latitude = cinema.Latitude,
                Longitude = cinema.Longitude,
                BannerUrl = cinema.BannerUrl,
                TotalScreens = cinema.TotalScreens,
                Facilities = cinema.Facilities,
                IsActive = cinema.IsActive,
                CreatedAt = cinema.CreatedAt,
                UpdatedAt = cinema.UpdatedAt,
                CurrentlyShowingMovies = moviesByCinema.TryGetValue(cinema.Id, out var movies)
                    ? movies
                    : new List<MovieResponseDto>()
            })
            .OrderBy(c => c.Name)
            .ToList();

        return result;
    }

    /// <summary>
    /// Lấy tất cả phim và lịch chiếu tại một rạp cụ thể (từ thời điểm hiện tại trở đi)
    /// </summary>
    public async Task<CinemaMoviesWithShowtimesResponseDto?> GetMoviesWithShowtimesAsync(
        Guid cinemaId,
        CancellationToken cancellationToken = default)
    {
        return await GetMoviesWithShowtimesInternalAsync(cinemaId, null, cancellationToken);
    }

    /// <summary>
    /// Lấy phim và lịch chiếu tại rạp theo ngày cụ thể
    /// </summary>
    public async Task<CinemaMoviesWithShowtimesResponseDto?> GetMoviesWithShowtimesByDateAsync(
        Guid cinemaId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        return await GetMoviesWithShowtimesInternalAsync(cinemaId, date.Date, cancellationToken);
    }

    private async Task<CinemaMoviesWithShowtimesResponseDto?> GetMoviesWithShowtimesInternalAsync(
        Guid cinemaId,
        DateTime? filterDate,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Kiểm tra rạp tồn tại và active
        var cinema = await _cinemaRepository.GetByIdAsync(cinemaId, cancellationToken);
        if (cinema == null || !cinema.IsActive)
            return null;

        // Query showtimes tại rạp
        var query = _context.Showtimes
            .AsNoTracking()
            .Where(s => s.Screen.CinemaId == cinemaId
                        && s.IsActive
                        && s.EndTime > now);

        if (filterDate.HasValue)
        {
            var dateUtc = DateTime.SpecifyKind(
                filterDate.Value.Date,
                DateTimeKind.Utc
            );

            query = query.Where(s =>
                s.StartTime >= dateUtc &&
                s.StartTime < dateUtc.AddDays(1)
            );
        }
        else
        {
            // Giới hạn trong 14 ngày tới để tránh load quá nhiều dữ liệu
            query = query.Where(s => s.StartTime <= now.AddDays(7));
        }

        var showtimes = await query
            .Include(s => s.Movie)
            .Include(s => s.Screen)
            .OrderBy(s => s.Movie.Title)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        if (!showtimes.Any())
        {
            return new CinemaMoviesWithShowtimesResponseDto
            {
                CinemaId = cinema.Id,
                CinemaName = cinema.Name,
                Slug = cinema.Slug ?? string.Empty,
                Address = cinema.Address,
                Movies = new List<MovieWithShowtimesDto>()
            };
        }

        // Group theo Movie để gom lịch chiếu
        var grouped = showtimes
            .GroupBy(s => s.MovieId)
            .Select(g => new MovieWithShowtimesDto
            {
                Movie = new MovieResponseDto
                {
                    Id = g.First().Movie.Id,
                    Title = g.First().Movie.Title,
                    Slug = g.First().Movie.Slug,
                    Description = g.First().Movie.Description,
                    PosterUrl = g.First().Movie.PosterUrl,
                    TrailerUrl = g.First().Movie.TrailerUrl,
                    DurationMinutes = g.First().Movie.DurationMinutes,
                    ReleaseDate = g.First().Movie.ReleaseDate,
                    Category = g.First().Movie.Category.ToString(),
                    Status = g.First().Movie.Status.ToString(),
                    CreatedAt = g.First().Movie.CreatedAt
                },
                Showtimes = g.Select(s => new ShowtimeResponseDto
                    {
                        Id = s.Id,
                        MovieId = s.MovieId,
                        ScreenId = s.ScreenId,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        BasePrice = s.BasePrice,
                        IsActive = s.IsActive
                    })
                    .OrderBy(st => st.StartTime)
                    .ToList()
            })
            .OrderBy(m => m.Movie.Title)
            .ToList();

        return new CinemaMoviesWithShowtimesResponseDto
        {
            CinemaId = cinema.Id,
            CinemaName = cinema.Name,
            Slug = cinema.Slug ?? string.Empty,
            Movies = grouped
        };
    }

    private static CinemaResponseDto MapToResponseDto(Cinema cinema)
    {
        return new CinemaResponseDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Description = cinema.Description,
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