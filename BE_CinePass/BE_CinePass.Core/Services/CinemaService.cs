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
                City = cinema.City,
                Latitude = cinema.Latitude,
                Longitude = cinema.Longitude,
                Movies = new List<MovieWithShowtimesDto>()
            };
        }

        // 3. Lấy danh sách MovieId
        var movieIds = showtimes
            .Select(s => s.MovieId)
            .Distinct()
            .ToList();

        var ratings = await _context.MovieReviews
            .AsNoTracking()
            .Where(r =>
                r.MovieId.HasValue &&
                r.Rating.HasValue &&
                movieIds.Contains(r.MovieId.Value)
            )
            .GroupBy(r => r.MovieId!.Value)
            .Select(g => new
            {
                MovieId = g.Key,
                AverageRating = g.Average(x => x.Rating!.Value),
                TotalReviews = g.Count()
            })
            .ToDictionaryAsync(
                x => x.MovieId,
                x => new
                {
                    x.AverageRating,
                    x.TotalReviews
                },
                cancellationToken
            );

        // Group theo Movie để gom lịch chiếu
        var grouped = showtimes
            .GroupBy(s => s.MovieId)
            .Select(g =>
            {
                var movie = g.First().Movie;
                ratings.TryGetValue(movie.Id, out var rating);

                return new MovieWithShowtimesDto
                {
                    Movie = new MovieResponseDto
                    {
                        Id = movie.Id,
                        Title = movie.Title,
                        Slug = movie.Slug,
                        Description = movie.Description,
                        PosterUrl = movie.PosterUrl,
                        TrailerUrl = movie.TrailerUrl,
                        DurationMinutes = movie.DurationMinutes,
                        ReleaseDate = movie.ReleaseDate,
                        Category = movie.Category.ToString(),
                        Status = movie.Status.ToString(),
                        CreatedAt = movie.CreatedAt,
                        AgeLimit = movie.AgeLimit,

                        // ⭐ Rating
                        AverageRating = rating?.AverageRating ?? 0,
                        TotalReviews = rating?.TotalReviews ?? 0
                    },
                    Showtimes = g
                        .Select(s => new ShowtimeResponseDto
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
                };
            })
            .OrderBy(x => x.Movie.Title)
            .ToList();

        // 6. Response
        return new CinemaMoviesWithShowtimesResponseDto
        {
            CinemaId = cinema.Id,
            CinemaName = cinema.Name,
            Slug = cinema.Slug ?? string.Empty,
            Address = cinema.Address,
            City = cinema.City,
            Latitude = cinema.Latitude,
            Longitude = cinema.Longitude,
            Movies = grouped
        };
    }

    /// <summary>
    /// Lấy danh sách các brand rạp chiếu phim (nhóm theo số điện thoại trùng nhau)
    /// Chỉ lấy các rạp đang hoạt động (IsActive = true)
    /// </summary>
    public async Task<List<CinemaBrandResponseDto>> GetCinemaBrandsAsync(CancellationToken cancellationToken = default)
    {
        var activeCinemas = await _cinemaRepository.GetActiveCinemasAsync(cancellationToken);

        // Nhóm theo Phone (loại bỏ khoảng trắng và ký tự đặc biệt để chuẩn hóa)
        var groupedByPhone = activeCinemas
            .Where(c => !string.IsNullOrWhiteSpace(c.Phone))
            .GroupBy(c => NormalizePhone(c.Phone))
            .Where(g => g.Key != null)
            .Select(g => new
            {
                PhoneKey = g.Key,
                Cinemas = g.OrderBy(c => c.City).ThenBy(c => c.Name).ToList()
            })
            .OrderByDescending(g => g.Cinemas.Count) // Brand lớn trước
            .ToList();

        // Xử lý các rạp không có số điện thoại hoặc số điện thoại duy nhất (có thể là rạp độc lập)
        var cinemasWithoutPhone = activeCinemas
            .Where(c => string.IsNullOrWhiteSpace(c.Phone))
            .OrderBy(c => c.Name)
            .ToList();

        var result = new List<CinemaBrandResponseDto>();

        // Các brand có số điện thoại
        foreach (var group in groupedByPhone)
        {
            var firstCinema = group.Cinemas.First();
            var brandName = ExtractBrandName(firstCinema.Name) ?? $"Hệ thống rạp ({group.Cinemas.Count} cụm)";

            result.Add(new CinemaBrandResponseDto
            {
                BrandName = brandName,
                Phone = firstCinema.Phone,
                TotalCinemas = group.Cinemas.Count,
                Cinemas = group.Cinemas.Select(MapToResponseDto).ToList()
            });
        }

        // Các rạp độc lập (không có phone hoặc phone duy nhất không trùng ai)
        foreach (var cinema in cinemasWithoutPhone)
        {
            result.Add(new CinemaBrandResponseDto
            {
                BrandName = cinema.Name, // Tự xem là brand riêng
                Phone = cinema.Phone,
                TotalCinemas = 1,
                Cinemas = new List<CinemaResponseDto> { MapToResponseDto(cinema) }
            });
        }

        return result;
    }

    /// <summary>
    /// Chuẩn hóa số điện thoại để so sánh (loại bỏ khoảng trắng, dấu gạch, ngoặc...)
    /// </summary>
    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        return System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");
    }

    /// <summary>
    /// Trích xuất tên brand từ tên rạp (ví dụ: "CGV Vincom Mega Mall" → "CGV")
    /// Có thể cải thiện bằng cách dùng danh sách từ khóa hoặc ML sau này
    /// </summary>
    private static string? ExtractBrandName(string cinemaName)
    {
        if (string.IsNullOrWhiteSpace(cinemaName))
            return null;

        var parts = cinemaName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var firstWord = parts[0].ToUpper();

        // Danh sách các brand phổ biến ở Việt Nam (có thể mở rộng)
        var knownBrands = new HashSet<string>
        {
            "CGV", "LOTTE", "GALAXY", "BHD", "CINEMA", "STARLIGHT", "MEGAGS", "CINEMAX", "PLATINUM", "BETACINEPLEX",
            "CINESTAR"
        };

        return knownBrands.Contains(firstWord) ? firstWord : parts[0];
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