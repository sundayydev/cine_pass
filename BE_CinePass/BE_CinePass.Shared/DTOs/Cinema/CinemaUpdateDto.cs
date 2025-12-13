using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Cinema;

public class CinemaUpdateDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public bool? IsActive { get; set; }
}

