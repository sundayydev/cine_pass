using BE_CinePass.Core.Repositories;
using BE_CinePass.Core.Configurations;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.Common;
using BE_CinePass.Shared.DTOs.Product;

namespace BE_CinePass.Core.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;
    private readonly ApplicationDbContext _context;

    public ProductService(ProductRepository productRepository, ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _context = context;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product == null ? null : MapToResponseDto(product);
    }

    public async Task<List<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(MapToResponseDto).ToList();
    }

    public async Task<List<ProductResponseDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetActiveProductsAsync(cancellationToken);
        return products.Select(MapToResponseDto).ToList();
    }

    public async Task<List<ProductResponseDto>> GetByCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetByCategoryAsync((Domain.Common.ProductCategory)category, cancellationToken);
        return products.Select(MapToResponseDto).ToList();
    }

    public async Task<List<ProductResponseDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.SearchAsync(searchTerm, cancellationToken);
        return products.Select(MapToResponseDto).ToList();
    }

    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Category = (Domain.Common.ProductCategory)dto.Category,
            IsActive = dto.IsActive
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(product);
    }

    public async Task<ProductResponseDto?> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            product.Name = dto.Name;

        if (dto.Description != null)
            product.Description = dto.Description;

        if (dto.Price.HasValue)
            product.Price = dto.Price.Value;

        if (dto.ImageUrl != null)
            product.ImageUrl = dto.ImageUrl;

        if (dto.Category.HasValue)
            product.Category = (Domain.Common.ProductCategory)dto.Category.Value;

        if (dto.IsActive.HasValue)
            product.IsActive = dto.IsActive.Value;

        _productRepository.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productRepository.RemoveByIdAsync(id, cancellationToken);
        if (result)
            await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            Category = product.Category.ToString(),
            IsActive = product.IsActive
        };
    }
}

