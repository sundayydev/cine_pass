using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Actor;

namespace BE_CinePass.Core.Services;

public class ActorService
{
    private readonly ActorRepository _actorRepository;
    private readonly ApplicationDbContext _context;

    public ActorService(ActorRepository actorRepository, ApplicationDbContext context)
    {
        _actorRepository = actorRepository;
        _context = context;
    }

    public async Task<ActorResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await _actorRepository.GetByIdAsync(id, cancellationToken);
        return actor == null ? null : MapToResponseDto(actor);
    }

    public async Task<ActorResponseDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var actor = await _actorRepository.GetBySlugAsync(slug, cancellationToken);
        return actor == null ? null : MapToResponseDto(actor);
    }

    public async Task<List<ActorResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var actors = await _actorRepository.GetAllAsync(cancellationToken);
        return actors.Select(MapToResponseDto).ToList();
    }

    public async Task<List<ActorResponseDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var actors = await _actorRepository.SearchAsync(searchTerm, cancellationToken);
        return actors.Select(MapToResponseDto).ToList();
    }

    public async Task<ActorResponseDto> CreateAsync(ActorCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Generate slug if not provided
        var slug = dto.Slug ?? GenerateSlug(dto.Name);

        // Check if slug exists
        if (await _actorRepository.SlugExistsAsync(slug, cancellationToken))
            throw new InvalidOperationException($"Slug {slug} đã tồn tại");

        var actor = new Actor
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _actorRepository.AddAsync(actor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(actor);
    }

    public async Task<ActorResponseDto?> UpdateAsync(Guid id, ActorUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var actor = await _actorRepository.GetByIdAsync(id, cancellationToken);
        if (actor == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            actor.Name = dto.Name;

        if (dto.Slug != null)
        {
            if (await _actorRepository.SlugExistsAsync(dto.Slug, cancellationToken) && actor.Slug != dto.Slug)
                throw new InvalidOperationException($"Slug {dto.Slug} đã tồn tại");
            actor.Slug = dto.Slug;
        }

        if (dto.Description != null)
            actor.Description = dto.Description;

        if (dto.ImageUrl != null)
            actor.ImageUrl = dto.ImageUrl;

        actor.UpdatedAt = DateTime.UtcNow;

        _actorRepository.Update(actor);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(actor);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _actorRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
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

    private static ActorResponseDto MapToResponseDto(Actor actor)
    {
        return new ActorResponseDto
        {
            Id = actor.Id,
            Name = actor.Name,
            Slug = actor.Slug,
            Description = actor.Description,
            ImageUrl = actor.ImageUrl,
            CreatedAt = actor.CreatedAt,
            UpdatedAt = actor.UpdatedAt
        };
    }
}
