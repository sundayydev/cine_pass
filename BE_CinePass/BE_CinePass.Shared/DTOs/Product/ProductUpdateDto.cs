using BE_CinePass.Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.Product;

public class ProductUpdateDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Range(0.01, 999999.99)]
    public decimal? Price { get; set; }

    [MaxLength(500)]
    [Url]
    public string? ImageUrl { get; set; }

    public ProductCategory? Category { get; set; }

    public bool? IsActive { get; set; }
}

