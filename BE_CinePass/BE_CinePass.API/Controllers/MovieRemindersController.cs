using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs;
using BE_CinePass.Shared.DTOs.Common;
using BE_CinePass.Shared.DTOs.Notification;
using BE_CinePass.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.API.Controllers;

[ApiController]
[Route("api/movie-reminders")]
[Authorize]
public class MovieReminderController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ILogger<MovieReminderController> _logger;

    public MovieReminderController(
        ApplicationDbContext context,
        IEventBus eventBus,
        ILogger<MovieReminderController> logger)
    {
        _context = context;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký nhận thông báo khi phim ra mắt
    /// </summary>
    [HttpPost("{movieId}/subscribe")]
    [ProducesResponseType(typeof(ApiResponseDto<MovieReminderResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> SubscribeToMovieReminder(
        Guid movieId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<object>.ErrorResult("Unauthorized"));

            // Check if movie exists
            var movie = await _context.Set<Movie>()
                .FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken);

            if (movie == null)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy phim"));

            // Check if already subscribed
            var existingReminder = await _context.Set<MovieReminder>()
                .FirstOrDefaultAsync(
                    mr => mr.MovieId == movieId && mr.UserId == userId.Value,
                    cancellationToken);

            if (existingReminder != null)
            {
                return Ok(ApiResponseDto<MovieReminderResponseDto>.SuccessResult(
                    new MovieReminderResponseDto
                    {
                        Id = existingReminder.Id,
                        MovieId = existingReminder.MovieId,
                        UserId = existingReminder.UserId,
                        IsActive = existingReminder.IsActive,
                        CreatedAt = existingReminder.CreatedAt
                    },
                    "Bạn đã đăng ký nhận thông báo cho phim này"));
            }

            // Create new reminder subscription
            var reminder = new MovieReminder
            {
                Id = Guid.NewGuid(),
                MovieId = movieId,
                UserId = userId.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Set<MovieReminder>().AddAsync(reminder, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} subscribed to movie {MovieId} ({MovieTitle})",
                userId.Value,
                movieId,
                movie.Title);

            return Ok(ApiResponseDto<MovieReminderResponseDto>.SuccessResult(
                new MovieReminderResponseDto
                {
                    Id = reminder.Id,
                    MovieId = reminder.MovieId,
                    UserId = reminder.UserId,
                    IsActive = reminder.IsActive,
                    CreatedAt = reminder.CreatedAt
                },
                "Đã đăng ký nhận thông báo thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to movie reminder");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi đăng ký nhận thông báo"));
        }
    }

    /// <summary>
    /// Hủy nhận thông báo khi phim ra mắt
    /// </summary>
    [HttpDelete("{movieId}/unsubscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UnsubscribeFromMovieReminder(
        Guid movieId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<object>.ErrorResult("Unauthorized"));

            var reminder = await _context.Set<MovieReminder>()
                .FirstOrDefaultAsync(
                    mr => mr.MovieId == movieId && mr.UserId == userId.Value,
                    cancellationToken);

            if (reminder == null)
                return NotFound(ApiResponseDto<object>.ErrorResult("Không tìm thấy đăng ký nhận thông báo"));

            _context.Set<MovieReminder>().Remove(reminder);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} unsubscribed from movie {MovieId}",
                userId.Value,
                movieId);

            return Ok(ApiResponseDto<object>.SuccessResult("Đã hủy nhận thông báo"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from movie reminder");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Lỗi khi hủy nhận thông báo"));
        }
    }

    /// <summary>
    /// Kiểm tra xem user đã đăng ký nhận thông báo cho phim này chưa
    /// </summary>
    [HttpGet("{movieId}/status")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetReminderStatus(
        Guid movieId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<bool>.ErrorResult("Unauthorized"));

            var isSubscribed = await _context.Set<MovieReminder>()
                .AnyAsync(
                    mr => mr.MovieId == movieId && mr.UserId == userId.Value && mr.IsActive,
                    cancellationToken);

            return Ok(ApiResponseDto<bool>.SuccessResult(isSubscribed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminder status");
            return StatusCode(500, ApiResponseDto<bool>.ErrorResult("Lỗi khi kiểm tra trạng thái"));
        }
    }

    /// <summary>
    /// Lấy danh sách phim mà user đã đăng ký nhận thông báo
    /// </summary>
    [HttpGet("my-reminders")]
    [ProducesResponseType(typeof(ApiResponseDto<List<MovieReminderDetailDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMyReminders(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponseDto<List<MovieReminderDetailDto>>.ErrorResult("Unauthorized"));

            var reminders = await _context.Set<MovieReminder>()
                .Include(mr => mr.Movie)
                .Where(mr => mr.UserId == userId.Value && mr.IsActive)
                .Select(mr => new MovieReminderDetailDto
                {
                    Id = mr.Id,
                    MovieId = mr.MovieId,
                    MovieTitle = mr.Movie.Title,
                    MoviePosterUrl = mr.Movie.PosterUrl,
                    ReleaseDate = mr.Movie.ReleaseDate,
                    CreatedAt = mr.CreatedAt
                })
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync(cancellationToken);

            return Ok(ApiResponseDto<List<MovieReminderDetailDto>>.SuccessResult(
                reminders,
                $"Tìm thấy {reminders.Count} phim đã đăng ký"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user reminders");
            return StatusCode(500, ApiResponseDto<List<MovieReminderDetailDto>>.ErrorResult("Lỗi khi lấy danh sách"));
        }
    }
}