namespace BE_CinePass.Shared.DTOs.Cinema;

public class CinemaResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
}

