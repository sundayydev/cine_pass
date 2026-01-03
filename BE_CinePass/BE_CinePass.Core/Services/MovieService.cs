using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Events;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Movie;
using BE_CinePass.Shared.DTOs.Showtime;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class MovieService
{
    private readonly MovieRepository _movieRepository;
    private readonly ApplicationDbContext _context;
    private readonly IEventBus _eventBus;

    public MovieService(MovieRepository movieRepository, ApplicationDbContext context,IEventBus eventBus)
    {
        _movieRepository = movieRepository;
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<MovieDetailResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        return movie == null ? null : MapToDetailResponseDto(movie);
    }

    public async Task<MovieDetailResponseDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetBySlugAsync(slug, cancellationToken);
        return movie == null ? null : MapToDetailResponseDto(movie);
    }

    public async Task<List<MovieResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetAllAsync(cancellationToken);
        return movies.Select(MapToResponseDto).ToList();
    }

    public async Task<List<MovieResponseDto>> GetNowShowingAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetNowShowingAsync(cancellationToken);
        return movies.Select(MapToResponseDto).ToList();
    }

    public async Task<List<MovieResponseDto>> GetComingSoonAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetComingSoonAsync(cancellationToken);
        return movies.Select(MapToResponseDto).ToList();
    }

    public async Task<List<MovieResponseDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.SearchAsync(searchTerm, cancellationToken);
        return movies.Select(MapToResponseDto).ToList();
    }

    public async Task<MovieResponseDto> CreateAsync(MovieCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Generate slug if not provided
        var slug = dto.Slug ?? GenerateSlug(dto.Title);

        // Check if slug exists
        if (await _movieRepository.SlugExistsAsync(slug, cancellationToken))
            throw new InvalidOperationException($"Slug {slug} đã tồn tại");

        var movie = new Movie
        {
            Title = dto.Title,
            Slug = slug,
            DurationMinutes = dto.DurationMinutes,
            Description = dto.Description,
            AgeLimit = dto.AgeLimit,
            PosterUrl = dto.PosterUrl,
            TrailerUrl = dto.TrailerUrl,
            ReleaseDate = dto.ReleaseDate,
            Category = (Domain.Common.MovieCategory)dto.Category,
            Status = (Domain.Common.MovieStatus)dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _movieRepository.AddAsync(movie, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        if (dto.Status == MovieStatus.Showing && movie.Status != Domain.Common.MovieStatus.Showing)
        {
            // 🔔 Publish MovieReleasedEvent
            await _eventBus.PublishAsync(new MovieReleasedEvent
            {
                MovieId = movie.Id,
                Title = movie.Title,
                PosterUrl = movie.PosterUrl,
                ReleaseDate = movie.ReleaseDate ?? DateTime.UtcNow
            });
        }
        return MapToResponseDto(movie);
    }

    public async Task<MovieResponseDto?> UpdateAsync(Guid id, MovieUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Title))
            movie.Title = dto.Title;

        if (dto.Slug != null)
        {
            if (await _movieRepository.SlugExistsAsync(dto.Slug, cancellationToken) && movie.Slug != dto.Slug)
                throw new InvalidOperationException($"Slug {dto.Slug} đã tồn tại");
            movie.Slug = dto.Slug;
        }

        if (dto.DurationMinutes.HasValue)
            movie.DurationMinutes = dto.DurationMinutes.Value;

        if (dto.Description != null)
            movie.Description = dto.Description;

        if (dto.PosterUrl != null)
            movie.PosterUrl = dto.PosterUrl;

        if (dto.TrailerUrl != null)
            movie.TrailerUrl = dto.TrailerUrl;

        if (dto.ReleaseDate.HasValue)
            movie.ReleaseDate = dto.ReleaseDate;

        if (dto.Category.HasValue)
            movie.Category = (Domain.Common.MovieCategory)dto.Category.Value;

        if (dto.Status.HasValue)
            movie.Status = (Domain.Common.MovieStatus)dto.Status.Value;

        _movieRepository.Update(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(movie);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _movieRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }
    public async Task<MovieCinemasWithShowtimesResponseDto?> GetCinemasWithShowtimesAsync(
        Guid movieId,
        DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        var query = _context.Showtimes
            .AsNoTracking()
            .Include(s => s.Screen)
            .ThenInclude(sc => sc.Cinema)
            .Include(s => s.Movie)
            .Where(s => s.MovieId == movieId && s.IsActive);

        if (date.HasValue)
        {
            var dateUtc = DateTime.SpecifyKind(
                date.Value.Date,
                DateTimeKind.Utc
            );

            var nextDateUtc = dateUtc.AddDays(1);

            // Lọc theo ngày và chỉ lấy suất chiếu chưa qua
            query = query.Where(s =>
                s.StartTime >= dateUtc &&
                s.StartTime < nextDateUtc &&
                s.StartTime > now  // Chỉ lấy suất chiếu trong tương lai
            );
        }
        else
        {
            // Lấy tất cả suất chiếu từ giờ hiện tại trở đi
            query = query.Where(s => s.StartTime > now);
        }


        var showtimes = await query
            .OrderBy(s => s.Screen.Cinema.Name)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        if (!showtimes.Any())
            return null;

        var movie = showtimes.First().Movie;

        var grouped = showtimes
            .GroupBy(s => s.Screen.Cinema)
            .Select(g => new CinemaWithShowtimesForMovieDto
            {
                CinemaId = g.Key.Id,
                CinemaName = g.Key.Name,
                Slug = g.Key.Slug ?? string.Empty,
                Address = g.Key.Address,
                City = g.Key.City,
                Phone = g.Key.Phone,
                BannerUrl = g.Key.BannerUrl,
                TotalScreens = g.Key.TotalScreens,
                Latitude = g.Key.Latitude,
                Longitude = g.Key.Longitude,
                Showtimes = g.Select(s => new ShowtimeResponseDto
                {
                    Id = s.Id,
                    ScreenId = s.ScreenId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    BasePrice = s.BasePrice,
                    IsActive = s.IsActive
                }).OrderBy(st => st.StartTime).ToList()
            })
            .OrderBy(c => c.CinemaName)
            .ToList();

        return new MovieCinemasWithShowtimesResponseDto
        {
            MovieId = movieId,
            MovieTitle = movie.Title,
            PosterUrl = movie.PosterUrl,
            Category = movie.Category.ToString(),
            DurationMinutes = movie.DurationMinutes,
            AgeLimit = movie.AgeLimit,
            Cinemas = grouped
        };
    }

    private static string GenerateSlug(string title)
    {
        return title.ToLower()
            .Replace(" ", "-")
            .Replace("đ", "d")
            .Replace("Đ", "D")
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
            .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");
    }

    private static MovieResponseDto MapToResponseDto(Movie movie)
    {
        var reviews = movie.MovieReviews?.Where(r => r.Rating.HasValue).ToList() ?? new List<MovieReview>();
        var totalReviews = reviews.Count;
        var averageRating = totalReviews > 0 
            ? Math.Round(reviews.Average(r => r.Rating!.Value), 1) 
            : (double?)null;

        return new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            DurationMinutes = movie.DurationMinutes,
            Description = movie.Description,
            AgeLimit = movie.AgeLimit,
            PosterUrl = movie.PosterUrl,
            TrailerUrl = movie.TrailerUrl,
            ReleaseDate = movie.ReleaseDate,
            Category = movie.Category.ToString(),
            Status = movie.Status.ToString(),
            CreatedAt = movie.CreatedAt,
            AverageRating = averageRating,
            TotalReviews = totalReviews
        };
    }

    private static MovieDetailResponseDto MapToDetailResponseDto(Movie movie)
    {
        var reviews = movie.MovieReviews?.Where(r => r.Rating.HasValue).ToList() ?? new List<MovieReview>();
        var totalReviews = reviews.Count;
        var averageRating = totalReviews > 0 
            ? Math.Round(reviews.Average(r => r.Rating!.Value), 1) 
            : (double?)null;

        return new MovieDetailResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            DurationMinutes = movie.DurationMinutes,
            Description = movie.Description,
            AgeLimit = movie.AgeLimit,
            PosterUrl = movie.PosterUrl,
            TrailerUrl = movie.TrailerUrl,
            ReleaseDate = movie.ReleaseDate,
            Category = movie.Category.ToString(),
            Status = movie.Status.ToString(),
            CreatedAt = movie.CreatedAt,
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            Actors = movie.MovieActors?.Select(ma => new MovieActorDto
            {
                Id = ma.Actor?.Id ?? Guid.Empty,
                Name = ma.Actor?.Name ?? string.Empty,
                Slug = ma.Actor?.Slug,
                Description = ma.Actor?.Description,
                ImageUrl = ma.Actor?.ImageUrl
            }).ToList() ?? new List<MovieActorDto>(),
            Reviews = movie.MovieReviews?.Select(mr => new MovieReviewDto
            {
                Id = mr.Id,
                UserId = mr.UserId,
                UserName = mr.User?.FullName ?? mr.User?.Email,
                Rating = mr.Rating,
                Comment = mr.Comment,
                CreatedAt = mr.CreatedAt
            }).ToList() ?? new List<MovieReviewDto>()
        };
    }
}

