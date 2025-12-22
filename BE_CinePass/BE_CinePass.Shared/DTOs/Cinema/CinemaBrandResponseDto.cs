namespace BE_CinePass.Shared.DTOs.Cinema;

public class CinemaBrandResponseDto
{
    public string BrandName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int TotalCinemas { get; set; }
    public List<CinemaResponseDto> Cinemas { get; set; } = new();
}