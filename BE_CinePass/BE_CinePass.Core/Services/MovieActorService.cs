using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.MovieActor;

namespace BE_CinePass.Core.Services;

public class MovieActorService
{
    private readonly MovieActorRepository _movieActorRepository;
    private readonly ApplicationDbContext _context;

    public MovieActorService(MovieActorRepository movieActorRepository, ApplicationDbContext context)
    {
        _movieActorRepository = movieActorRepository;
        _context = context;
    }

    public async Task<MovieActorResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movieActor = await _movieActorRepository.GetByIdAsync(id, cancellationToken);
        return movieActor == null ? null : MapToResponseDto(movieActor);
    }

    public async Task<List<MovieActorResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movieActors = await _movieActorRepository.GetAllAsync(cancellationToken);
        return movieActors.Select(MapToResponseDto).ToList();
    }

    public async Task<List<MovieActorResponseDto>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var movieActors = await _movieActorRepository.GetByMovieIdAsync(movieId, cancellationToken);
        return movieActors.Select(MapToResponseDto).ToList();
    }

    public async Task<List<MovieActorResponseDto>> GetByActorIdAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        var movieActors = await _movieActorRepository.GetByActorIdAsync(actorId, cancellationToken);
        return movieActors.Select(MapToResponseDto).ToList();
    }

    public async Task<MovieActorResponseDto> CreateAsync(MovieActorCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Check if relationship already exists
        if (await _movieActorRepository.ExistsAsync(dto.MovieId, dto.ActorId, cancellationToken))
            throw new InvalidOperationException($"Diễn viên này đã được thêm vào phim");

        var movieActor = new MovieActor
        {
            MovieId = dto.MovieId,
            ActorId = dto.ActorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _movieActorRepository.AddAsync(movieActor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with related data
        var created = await _movieActorRepository.GetByIdAsync(movieActor.Id, cancellationToken);
        return MapToResponseDto(created!);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _movieActorRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static MovieActorResponseDto MapToResponseDto(MovieActor movieActor)
    {
        return new MovieActorResponseDto
        {
            Id = movieActor.Id,
            MovieId = movieActor.MovieId,
            ActorId = movieActor.ActorId,
            CreatedAt = movieActor.CreatedAt,
            MovieTitle = movieActor.Movie?.Title,
            ActorName = movieActor.Actor?.Name
        };
    }
}
