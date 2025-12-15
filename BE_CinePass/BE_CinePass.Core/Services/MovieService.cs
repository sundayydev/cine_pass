using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Movie;

namespace BE_CinePass.Core.Services;

public class MovieService
{
    private readonly MovieRepository _movieRepository;
    private readonly ApplicationDbContext _context;

    public MovieService(MovieRepository movieRepository, ApplicationDbContext context)
    {
        _movieRepository = movieRepository;
        _context = context;
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
            CreatedAt = movie.CreatedAt
        };
    }

    private static MovieDetailResponseDto MapToDetailResponseDto(Movie movie)
    {
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

