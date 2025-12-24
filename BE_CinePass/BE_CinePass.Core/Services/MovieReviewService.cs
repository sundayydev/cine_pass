using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.MovieReview;
using Microsoft.EntityFrameworkCore;

namespace BE_CinePass.Core.Services;

public class MovieReviewService
{
    private readonly MovieReviewRepository _movieReviewRepository;
    private readonly ApplicationDbContext _context;

    public MovieReviewService(MovieReviewRepository movieReviewRepository, ApplicationDbContext context)
    {
        _movieReviewRepository = movieReviewRepository;
        _context = context;
    }

    public async Task<List<MovieReviewResponseDto>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var reviews = await _movieReviewRepository.GetByMovieIdAsync(movieId, cancellationToken);
        return reviews.Select(r => new MovieReviewResponseDto
        {
            Id = r.Id,
            MovieId = r.MovieId,
            UserId = r.UserId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            UserName = r.User?.FullName ?? r.User?.Email
        }).ToList();
    }

    public async Task<MovieReviewResponseDto> CreateAsync(Guid userId, MovieReviewCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (await _movieReviewRepository.HasUserReviewedAsync(userId, dto.MovieId, cancellationToken))
        {
            throw new InvalidOperationException("Bạn đã đánh giá phim này rồi");
        }

        var review = new MovieReview
        {
            MovieId = dto.MovieId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _movieReviewRepository.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Load user to return UserName
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

        return new MovieReviewResponseDto
        {
            Id = review.Id,
            MovieId = review.MovieId,
            UserId = review.UserId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            UserName = user?.FullName ?? user?.Email
        };
    }
}
